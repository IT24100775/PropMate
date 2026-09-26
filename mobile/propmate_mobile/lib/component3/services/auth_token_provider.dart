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

class TenantSession implements AuthTokenProvider {
  TenantSession._();

  static final TenantSession instance = TenantSession._();

  int? userId;
  String? email;
  String? _accessToken;

  @override
  String? get accessToken => _accessToken;

  bool get isSignedIn => userId != null && _accessToken != null;

  void signIn({required int id, required String address, required String token}) {
    userId = id;
    email = address;
    _accessToken = token;
  }

  void signOut() {
    userId = null;
    email = null;
    _accessToken = null;
  }
}
