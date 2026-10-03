/// Component 3 authentication boundary.
///
/// The existing mobile authentication implementation remains owned by the
/// main app. Inject an implementation of this interface once auth is ready.
abstract class AuthTokenProvider {
  String? get accessToken;
}

class EmptyAuthTokenProvider implements AuthTokenProvider {
  const EmptyAuthTokenProvider();

  @override
  String? get accessToken => null;
}
