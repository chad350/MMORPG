using System;
using System.Collections.Generic;

namespace Server.Game
{
    public class JobSerializer
    {
        // 미래에 할일들은 타이머에 등록하고
        // 지금 처리할 일들은 잡 큐에서 관리
        private JobTimer _timer = new JobTimer();
        Queue<IJob> _jobQueue = new Queue<IJob>();
        object _lock = new object();
        bool _flush = false;

        
        /// <summary>
        /// 미래의 등록할 일감
        /// </summary>
        /// <param name="tickAfter">몇초 뒤에 실행해야하는지 (ms 기준)</param>
        /// <param name="job">처리할 작업</param>
        public void PushAfter(int tickAfter, IJob job)
        {
            _timer.Push(job, tickAfter);
        }
        
        // 헬퍼 함수 - PushAfter
        public void PushAfter(int tickAfter, Action action) { PushAfter(tickAfter, new Job(action)); }
        public void PushAfter<T1>(int tickAfter, Action<T1> action, T1 t1) { PushAfter(tickAfter, new Job<T1>(action, t1)); }
        public void PushAfter<T1, T2>(int tickAfter, Action<T1, T2> action, T1 t1, T2 t2) { PushAfter(tickAfter, new Job<T1, T2>(action, t1, t2)); }
        public void PushAfter<T1, T2, T3>(int tickAfter, Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { PushAfter(tickAfter, new Job<T1, T2, T3>(action, t1, t2, t3)); }
        

        /// <summary>
        /// 현재 처리할 일감
        /// </summary>
        /// <param name="job">처리할 작업</param>
        public void Push(IJob job)
        {
            lock (_lock)
            {
                _jobQueue.Enqueue(job);
            }
        }
        
        // 헬퍼 함수 - Push
        public void Push(Action action) { Push(new Job(action)); }
        public void Push<T1>(Action<T1> action, T1 t1) { Push(new Job<T1>(action, t1)); }
        public void Push<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2) { Push(new Job<T1, T2>(action, t1, t2)); }
        public void Push<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { Push(new Job<T1, T2, T3>(action, t1, t2, t3)); }
        

        // 누군가가 주기적으로 호출해줘야 한다.
        public void Flush()
        {
            _timer.Flush();
            
            while (true)
            {
                IJob job = Pop();
                if (job == null)
                    return;

                job.Execute();
            }
        }

        IJob Pop()
        {
            lock (_lock)
            {
                if (_jobQueue.Count == 0)
                {
                    _flush = false;
                    return null;
                }
                return _jobQueue.Dequeue();
            }
        }
    }
}