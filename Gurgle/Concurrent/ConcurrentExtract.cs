using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Gurgle.Utils;
using FileHelpers;
using System.Threading.Tasks;

namespace Gurgle.Concurrent
{    
    public abstract class ConcurrentExtract<TSource,TKey,TRec> 
        where TRec : class
        where TKey : class
    {

        #region properties

        protected Type[] RecTypes
        { get; set; }
        
        private int _maxThreads = Util.MaxThreadDefault;
        public int MaxThreads
        {
            get { return _maxThreads; }
            set { _maxThreads = value; }
        }

        private int _batchSize = 25;
        public int AsyncBatchSize
        {
            get { return _batchSize; }
            set { _batchSize = value; }
        }

        private int _totalKeys;
        public int TotalItems
        { get { return _totalKeys; } }

        private int _completed;
        public int CompletedItems
        { get { return _completed; } }

        public double Progress
        { get { return (_totalKeys > 0) ? ((double)_completed / _totalKeys) * 100 : 0; } }
        
        #endregion

        #region events / callbacks

        public MapProvider<TSource, TRec> NewMapThreadHandler
        { get; set; }
        protected virtual IMapper<TSource, TRec> GetMapper()
        {
            if (NewMapThreadHandler == null)
                throw new InvalidOperationException("NewMapThreadHandler is null");

            return NewMapThreadHandler();
        }

        public event ExtractThreadCompleteHandler<TRec> ExtractThreadComplete;
        protected void OnExtractComplete(TRec[] records)
        {
            if (ExtractThreadComplete != null)
            {
                ExtractThreadComplete(this, new ExtractThreadCompleteEventArgs<TRec>(records));
            }
        }

        #endregion

        #region constructors

        protected ConcurrentExtract()
        {
            //override .Net connection pool defaults
            if (System.Net.ServicePointManager.DefaultConnectionLimit == System.Net.ServicePointManager.DefaultPersistentConnectionLimit)
                System.Net.ServicePointManager.DefaultConnectionLimit = Concurrent.Util.MaxConnectionDefault;
        }

        protected ConcurrentExtract(params Type[] RecordTypes)
            : this()
        {
            if (typeof(TRec) != typeof(object) && RecordTypes != null && RecordTypes.Length > 0)
                throw new InvalidOperationException("TRec must be object for multiple record types");
           
            RecTypes = RecordTypes;

            if (typeof(TRec) == typeof(object) &&
                (RecTypes == null || RecTypes.Length == 0))
                throw new InvalidOperationException("Cannot use record type 'object' without specifying a record type.");
                        
        }
        protected ConcurrentExtract(Func<IMapper<TSource, TRec>> IMapFactory, params Type[] RecordTypes)
            : this(RecordTypes)
        {
            this.NewMapThreadHandler = new MapProvider<TSource,TRec>(IMapFactory);
        }
        protected ConcurrentExtract(Func<IMapper<TSource, TRec>> IMapFactory)
            : this(new Type[]{})
        {
            this.NewMapThreadHandler = new MapProvider<TSource, TRec>(IMapFactory);
        }

        #endregion


        protected abstract Task<TSource> GetDataByKeyAsync(TKey key);
        protected abstract IEnumerable<TKey> GetEnumerableKeys();


        private List<TRec> allRecs;
        public virtual TRec[] ExtractRecords()
        {
            allRecs = new List<TRec>();
            this.ExtractThreadComplete += InternalExtractCompleteHandler;
            DoExtract();

            this.ExtractThreadComplete -= InternalExtractCompleteHandler;

            TRec[] rtnVal = allRecs.ToArray();
            allRecs = null;

            return rtnVal;
        }

        private readonly object locker = new object();
        private void InternalExtractCompleteHandler(object source, ExtractThreadCompleteEventArgs<TRec> e)
        {
            lock (locker)
            {
                allRecs.AddRange(e.Records);
            }
        }
        
        private ConcurrentWorkManager<IList<TKey>> workManager;  
        public void DoExtract()
        {
            if (workManager == null)
                workManager = new ConcurrentWorkManager<IList<TKey>>(MaxThreads, NewWorkerThread);

            Debug.WriteLine("Worker threads: {0}", MaxThreads);

            Thread outputThread = new Thread(OutputThread);
            outputThread.Start();
            

            Debug.WriteLine("Beginning get keys at: {0}", DateTime.Now);
            int bufferSize = _batchSize;
            TKey[] buffer = new TKey[bufferSize];
            int c = 0;
            foreach (TKey key in GetEnumerableKeys())
            {
                if (c == bufferSize)
                {
                    workManager.AddToQueue(buffer);
                    c = 0;
                    buffer = new TKey[bufferSize];
                }

                buffer[c] = key;
                Interlocked.Increment(ref _totalKeys);
                c++; //not the language
            }
            buffer = buffer.Where(k => k != null).ToArray();
            workManager.AddToQueue(buffer);
            
            Debug.WriteLine("Finished get keys at: {0}", DateTime.Now);
            Debug.WriteLine("Total keys to pull: {0}", _totalKeys);
        
            workManager.WaitForCompletion();

            outputThreadTerm = true;
            outputThread.Join();
            Debug.WriteLine("OutQueue count at close {0}", recordQueue.Count);

        }

        private Action<IList<TKey>> NewWorkerThread()
        {
            IMapper<TSource, TRec> RecMap = GetMapper();
            return keys =>
            {
                List<Task<TSource>> requests = new List<Task<TSource>>();
                foreach (TKey key in keys)
                {
                    requests.Add(GetDataByKeyAsync(key));
                }

                foreach (var task in Util.Interleaved(requests))
                {
                    Task<TSource> compTask = task.Result;
                    TSource data = compTask.Result;
                    if (!RecMap.Skip(data))
                        recordQueue.Enqueue(RecMap.Map(data));
                }
            };
        }
    
        private bool outputThreadTerm = false;
        private readonly ConcurrentQueue<TRec[]> recordQueue = new ConcurrentQueue<TRec[]>(); 
        private void OutputThread()
        {
            List<TRec> buffer = new List<TRec>();
            TRec[] mapResult;

            while (true)
            {
                if (!recordQueue.IsEmpty && recordQueue.TryDequeue(out mapResult))
                {
                    buffer.AddRange(mapResult);
                    Interlocked.Increment(ref _completed);
                    if (buffer.Count >= 1000)
                    {
                        OnExtractComplete(buffer.ToArray());
                        buffer.Clear();
                    }
                    continue;
                }

                if (outputThreadTerm && recordQueue.IsEmpty)
                    break;

                Thread.Sleep(500); //if the queue is empty and term signal hasn't been recieved, wait a moment for workers to fill queue
            }

            if (buffer.Count > 0)
                OnExtractComplete(buffer.ToArray());
        }

        #region extract to file
        public virtual void ExtractToFile(string OutputPath)
        {   
            TextWriter writer = new StreamWriter(OutputPath, false);
            Type recType = typeof(TRec);
            if (recType == typeof(object))
            {
                if (RecTypes.Length > 1)
                {
                    mFileHelperEngine = new MultiRecordEngine(RecTypes); //actually multiple record types
                    mFileHelperEngine.BeginWriteStream(writer);
                    multiRecEngine = true;
                }
                else if (RecTypes.Length == 1)
                {
                    recType = RecTypes[0];
                }
            }

            if (!multiRecEngine)
            {
                sFileHelperEngine = new FileHelperAsyncEngine(recType);
                sFileHelperEngine.BeginWriteStream(writer);
            }

            this.ExtractThreadComplete += WriteOutputThreadCompleteHandler;
            DoExtract();
            writer.Flush();
            writer.Close();
            this.ExtractThreadComplete -= WriteOutputThreadCompleteHandler;
        }

        private bool multiRecEngine = false;
        private FileHelperAsyncEngine sFileHelperEngine;
        private MultiRecordEngine mFileHelperEngine;
        private object writeLocker = new object();
        private void WriteOutputThreadCompleteHandler(object source, ExtractThreadCompleteEventArgs<TRec> e)
        {
            TRec[] recs = e.Records;
            lock (writeLocker)
            {
                if (multiRecEngine)
                    mFileHelperEngine.WriteNexts(recs);
                else
                    sFileHelperEngine.WriteNexts(recs);
            }
        }

        #endregion
        
    }
}
