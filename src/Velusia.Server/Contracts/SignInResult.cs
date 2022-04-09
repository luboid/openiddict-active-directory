namespace Velusia.Server.Contracts
{
	public class SignInResult
	{
		public SignInResult()
		{
		}

		public SignInResult(string failMessage)
		{
			Message = failMessage;
		}

		private static readonly SignInResult _success = new SignInResult
		{
			Succeeded = true
		};

		private static readonly SignInResult _failed = new SignInResult();

		private static readonly SignInResult _lockedOut = new SignInResult
		{
			IsLockedOut = true
		};

		private static readonly SignInResult _notAllowed = new SignInResult
		{
			IsNotAllowed = true
		};

		private static readonly SignInResult _twoFactorRequired = new SignInResult
		{
			RequiresTwoFactor = true
		};

		public bool Succeeded { get; protected set; }

		public bool IsLockedOut { get; protected set; }

		public bool IsNotAllowed { get; protected set; }

		public bool RequiresTwoFactor { get; protected set; }

		public string Message { get; protected set; }

		public static SignInResult Success => _success;

		public static SignInResult Failed => _failed;

		public static SignInResult LockedOut => _lockedOut;

		public static SignInResult NotAllowed => _notAllowed;

		public static SignInResult TwoFactorRequired => _twoFactorRequired;

		public override string ToString()
		{
			if (!IsLockedOut)
			{
				if (!IsNotAllowed)
				{
					if (!RequiresTwoFactor)
					{
						if (!Succeeded)
						{
							return "Failed";
						}
						return "Succeeded";
					}
					return "RequiresTwoFactor";
				}
				return "NotAllowed";
			}
			return "Lockedout";
		}
	}
}
