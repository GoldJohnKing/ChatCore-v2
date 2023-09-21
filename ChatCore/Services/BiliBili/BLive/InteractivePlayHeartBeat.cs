using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ChatCore.Utilities;

namespace OpenBLive.Runtime
{
    public delegate void HeartBeatSucceed();

    public delegate void HeartBeatError(string json);

    public class InteractivePlayHeartBeat : IDisposable
    {
        public event HeartBeatSucceed? HeartBeatSucceed;
        public event HeartBeatError? HeartBeatError;
        private readonly CancellationTokenSource m_Cancellation;
        private readonly string[] m_GameIds;
		private readonly int m_Time;

		public InteractivePlayHeartBeat(string gameId, int time = 20000, CancellationTokenSource? cancellation = null)
        {
            m_GameIds = new[] { gameId };
            m_Time = time;
            m_Cancellation = cancellation ?? new CancellationTokenSource();
        }

        public InteractivePlayHeartBeat(string[] gameIds, int time = 20000, CancellationTokenSource? cancellation = null)
        {
            m_GameIds = gameIds;
            m_Time = time;
            m_Cancellation = cancellation ?? new CancellationTokenSource();
        }

        private async Task HeartBeatTask()
        {
            while (true)
            {
				switch (m_Cancellation)
				{
					case { IsCancellationRequested: true }:
					case null:
						return;
					default:
						try
						{
							var res = "";
							if (m_GameIds.Length == 1)
							{
								res = await BApi.HeartBeatInteractivePlay(m_GameIds[0]);
							}
							else
							{
								res = await BApi.BatchHeartBeatInteractivePlay(m_GameIds);
							}

							var json = JSON.Parse(res);
							var code = json["code"].AsInt;
							if (code == 0)
							{
								HeartBeatSucceed?.Invoke();
							}
							else
							{
								HeartBeatError?.Invoke(res);
								return;
							}
						}
						catch (Exception e)
						{
							HeartBeatError?.Invoke(e.Message);
							return;
						}
						await Task.Delay(m_Time);
						break;
				}
            }
        }

        public void Start()
        {
            var task = HeartBeatTask();
            if (task.Status == TaskStatus.Created)
			{
				task.Start();
			}
		}

        public void Stop()
        {
            m_Cancellation.Cancel();
        }

        public void Dispose()
        {
            m_Cancellation.Dispose();
        }
    }
}