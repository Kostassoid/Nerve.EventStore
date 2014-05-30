namespace Kostassoid.Nerve.EventStore.Specs.Command
{
	using System;
	using System.Threading.Tasks;
	using Core;
	using Core.Processing.Operators;
	using Core.Scheduling;
	using Model;

	public class CommandHandler : Cell
	{
		readonly IEventStore _store;

		public CommandHandler(IEventStore store) : base("CommandHandler", ThreadScheduler.Factory)
		{
			_store = store;

			OnStream().Of<CreateUser>().ReactWith(CreateUserHandler);
			OnStream().Of<ChangeUserName>().ReactWith(ChangeUserNameHandler);
			OnStream().Of<CelebrateUserBirthday>().ReactWith(CelebrateUserBirthdayHandler);
		}

		static Task PerformWithRetry(Func<Task> action)
		{
			while (true)
			{
				try
				{
					return action();
				}
				catch (ConcurrencyException ex)
				{
					Console.WriteLine("Concurrency exception [{0}]. Retrying...", ex);
				}
			}
		}

		void CreateUserHandler(ISignal<CreateUser> signal)
		{
			Console.WriteLine("Creating user {0}.", signal.Payload.Name);

			var user = User.Create(signal.Payload.Id, signal.Payload.Name, signal.Payload.Age);
			_store.Commit(user).Wait();
			signal.Return("OK");
		}

		void ChangeUserNameHandler(ISignal<ChangeUserName> signal)
		{
			Func<Task> act = () =>
			{
				Console.WriteLine("Changing user name to {0}.", signal.Payload.NewName);

				var user = _store.Load<User>(signal.Payload.Id).Result;
				user.ChangeName(signal.Payload.NewName);
				return _store.Commit(user);
			};

			PerformWithRetry(act).Wait();
			signal.Return("OK");
		}

		void CelebrateUserBirthdayHandler(ISignal<CelebrateUserBirthday> signal)
		{
			Func<Task> act = () =>
			{
				//Console.WriteLine("Celebrating user birthday.");

				var user = _store.Load<User>(signal.Payload.Id).Result;
				user.Birthday();
				return _store.Commit(user);
			};

			PerformWithRetry(act).Wait();
			signal.Return("OK");
		}

		public override bool OnFailure(SignalException exception)
		{
			Console.WriteLine("Cought exception of type {0}", exception.InnerException.GetType());
			return true;
		}
	}
}