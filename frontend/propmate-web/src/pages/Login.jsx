
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/authContext";

function Login() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);

    const { login } = useAuth();
    const navigate = useNavigate();

    const handleSubmit = async (event) => {
        event.preventDefault();

        setError("");
        setLoading(true);

        try {
            const authenticatedUser = await login(email, password);

            if (authenticatedUser.role === "Admin") {
                navigate("/admin");
            } else if (authenticatedUser.role === "OwnerAgent") {
                navigate("/owner");
            } else {
                navigate("/discover");
            }
        } catch (err) {
            setError(err.message || "Unable to sign in.");
        } finally {
            setLoading(false);
        }
    };

    return (
        <main className="login-page">
            <section className="login-card">
                <h1>Sign in to PropMate</h1>

                <p>
                    Sign in to discover properties and book viewings.
                </p>

                {error && (
                    <p role="alert">
                        {error}
                    </p>
                )}

                <form onSubmit={handleSubmit}>
                    <div>
                        <label htmlFor="email">
                            Email address
                        </label>

                        <input
                            id="email"
                            type="email"
                            value={email}
                            onChange={(event) =>
                                setEmail(event.target.value)
                            }
                            placeholder="you@example.com"
                            required
                        />
                    </div>

                    <div>
                        <label htmlFor="password">
                            Password
                        </label>

                        <input
                            id="password"
                            type="password"
                            value={password}
                            onChange={(event) =>
                                setPassword(event.target.value)
                            }
                            placeholder="Enter your password"
                            required
                        />
                    </div>

                    <button
                        type="submit"
                        disabled={loading}
                    >
                        {loading ? "Signing in..." : "Sign in"}
                    </button>
                </form>

                <p>
                    New to PropMate?{" "}
                    <Link to="/register">
                        Create an account
                    </Link>
                </p>
            </section>
        </main>
    );
}

export default Login;