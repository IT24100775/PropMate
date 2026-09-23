
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/authContext";

function Register() {
    const [formData, setFormData] = useState({
        firstName: "",
        lastName: "",
        email: "",
        password: "",
    });

    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);

    const { register } = useAuth();
    const navigate = useNavigate();

    const handleChange = (event) => {
        setFormData({
            ...formData,
            [event.target.name]: event.target.value,
        });
    };

    const handleSubmit = async (event) => {
        event.preventDefault();

        setError("");
        setLoading(true);

        try {
            const authenticatedUser = await register(
                formData.firstName,
                formData.lastName,
                formData.email,
                formData.password
            );

            if (authenticatedUser.role === "Admin") {
                navigate("/admin");
            } else if (authenticatedUser.role === "OwnerAgent") {
                navigate("/owner");
            } else {
                navigate("/discover");
            }
        } catch (err) {
            setError(err.message || "Unable to create account.");
        } finally {
            setLoading(false);
        }
    };

    return (
        <main className="register-page">
            <section className="register-card">
                <h1>Create your account</h1>

                <p>
                    Join PropMate and start your property journey.
                </p>

                {error && (
                    <p role="alert">
                        {error}
                    </p>
                )}

                <form onSubmit={handleSubmit}>
                    <div>
                        <label htmlFor="firstName">
                            First name
                        </label>

                        <input
                            id="firstName"
                            name="firstName"
                            type="text"
                            value={formData.firstName}
                            onChange={handleChange}
                            required
                        />
                    </div>

                    <div>
                        <label htmlFor="lastName">
                            Last name
                        </label>

                        <input
                            id="lastName"
                            name="lastName"
                            type="text"
                            value={formData.lastName}
                            onChange={handleChange}
                            required
                        />
                    </div>

                    <div>
                        <label htmlFor="email">
                            Email address
                        </label>

                        <input
                            id="email"
                            name="email"
                            type="email"
                            value={formData.email}
                            onChange={handleChange}
                            required
                        />
                    </div>

                    <div>
                        <label htmlFor="password">
                            Password
                        </label>

                        <input
                            id="password"
                            name="password"
                            type="password"
                            value={formData.password}
                            onChange={handleChange}
                            minLength={8}
                            required
                        />
                    </div>

                    <button
                        type="submit"
                        disabled={loading}
                    >
                        {loading
                            ? "Creating account..."
                            : "Create account"}
                    </button>
                </form>

                <p>
                    Already have an account?{" "}
                    <Link to="/login">
                        Sign in
                    </Link>
                </p>
            </section>
        </main>
    );
}

export default Register;