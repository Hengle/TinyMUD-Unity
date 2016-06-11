using System;
using System.Collections.Generic;
using System.Threading;

namespace TinyMUD
{
	public abstract class Promise
	{
		public enum State
		{
			Pending,
			Complete,
			Error,
		}
	}

	public sealed class Promise<T> : Promise
	{
		private T result;
		private Exception exception;
		private int state;

		#region 内部使用的类
		private class HandlerAction
		{
			public readonly Action emit;
			public Promise<T> promise;

			private HandlerAction()
			{
				emit = () =>
				{
					if (promise.Callback != null)
						promise.Callback(promise);
					lock (pool)
					{
						pool.Push(this);
					}
				};
			}

			private static readonly Stack<HandlerAction> pool = new Stack<HandlerAction>();
			public static HandlerAction Acquire()
			{
				if (pool.Count > 0)
				{
					lock (pool)
					{
						if (pool.Count > 0)
							return pool.Pop();
					}
				}
				return new HandlerAction();
			}
		}
		#endregion

		public Promise()
		{
			state = (int)State.Pending;
		}

		public bool Complete(T t)
		{
			if (!TrySetState(State.Complete))
				return false;
			result = t;
			ExecCallback();
			return true;
		}

		public bool Error(Exception e)
		{
			if (!TrySetState(State.Error))
				return false;
			exception = e;
			ExecCallback();
			return true;
		}

		public event Action<Promise<T>> Callback;

		public T Result
		{
			get
			{
				if (state != (int)State.Complete)
					throw new InvalidOperationException();
				return result;
			}
		}

		public Exception Exception
		{
			get { return exception; }
		}

		public State CurrentState
		{
			get { return (State)state; }
		}

		public bool IsCompleted
		{
			get { return state == (int) State.Complete; }
		}

		public bool IsFaulted
		{
			get { return state == (int)State.Error; }
		}

		private bool TrySetState(State s)
		{
			if (state != (int)State.Pending)
				return false;
			if ((int)State.Pending != Interlocked.CompareExchange(ref state, (int)s, (int)State.Pending))
				return false;
			return true;
		}

		private void ExecCallback()
		{
			HandlerAction action = HandlerAction.Acquire();
			action.promise = this;
			Loop.Run(action.emit);
		}
	}
}