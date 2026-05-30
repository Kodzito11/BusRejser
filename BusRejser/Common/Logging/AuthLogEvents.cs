namespace BusRejser.Common.Logging;

public static class AuthLogEvents
{
	public const string RegisterAttempt = "AUTH_REGISTER_ATTEMPT";
	public const string RegisterSuccess = "AUTH_REGISTER_SUCCESS";
	public const string RegisterFailed = "AUTH_REGISTER_FAILED";

	public const string LoginAttempt = "AUTH_LOGIN_ATTEMPT";
	public const string LoginSuccess = "AUTH_LOGIN_SUCCESS";
	public const string LoginFailed = "AUTH_LOGIN_FAILED";

	public const string RefreshAttempt = "AUTH_REFRESH_ATTEMPT";
	public const string RefreshSuccess = "AUTH_REFRESH_SUCCESS";
	public const string RefreshFailed = "AUTH_REFRESH_FAILED";

	public const string LogoutAttempt = "AUTH_LOGOUT_ATTEMPT";
	public const string LogoutSuccess = "AUTH_LOGOUT_SUCCESS";
	public const string LogoutSkipped = "AUTH_LOGOUT_SKIPPED";

	public const string PasswordResetRequested = "AUTH_PASSWORD_RESET_REQUESTED";
	public const string PasswordResetSuccess = "AUTH_PASSWORD_RESET_SUCCESS";

	public const string EmailVerificationSuccess = "AUTH_EMAIL_VERIFICATION_SUCCESS";
	public const string VerificationEmailResent = "AUTH_VERIFICATION_EMAIL_RESENT";
}