import { useState, useEffect } from "react";
import useMusicCrud from "../useMusicCrud";
import { useAtomValue } from "jotai";
import { userAtom } from "../atoms/userAtom";
import { useNavigate } from "react-router-dom";

export default function SignUp() {
    const { createUser, getMe } = useMusicCrud();
    const user = useAtomValue(userAtom);
    const navigate = useNavigate();

    const [username, setUsername] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [passwordConfirm, setPasswordConfirm] = useState("");

    const [showPassword, setShowPassword] = useState(false);
    const [showConfirmPassword, setShowConfirmPassword] = useState(false);

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        getMe().catch(() => {});
    }, []);

    useEffect(() => {
        if (user) {
            navigate("/home");
        }
    }, [user]);

    async function handleSubmit(e: { preventDefault: () => void; }) {
        e.preventDefault();
        setError(null);

        if (password !== passwordConfirm) {
            setError("Passwords do not match");
            return;
        }

        setLoading(true);

        try {
            await createUser({ username, email, password, passwordConfirm });
            navigate("/home");
        } catch (err: any) {
            setError("Failed to create account");
        } finally {
            setLoading(false);
        }
    }

    return (
        <div className="min-h-screen flex items-center justify-center bg-base-200 px-4">
            <div className="w-full max-w-md">
                <div className="space-y-10">
                    <h2 className="text-3xl font-bold text-center text-base-content">
                        Sign Up
                    </h2>

                    <form className="space-y-6" onSubmit={handleSubmit}>
                        <div className="form-control">
                            <label className="label">
                                <span className="label-text text-base-content/90">
                                    Username
                                </span>
                            </label>
                            <input
                                type="text"
                                placeholder="Username"
                                value={username}
                                onChange={(e) => setUsername(e.target.value)}
                                className="input input-bordered input-lg w-full focus:input-primary bg-base-100/70 backdrop-blur-sm"
                                required
                            />
                        </div>

                        <div className="form-control">
                            <label className="label">
                                <span className="label-text text-base-content/90">
                                    Email
                                </span>
                            </label>
                            <input
                                type="email"
                                placeholder="Email"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                                className="input input-bordered input-lg w-full focus:input-primary bg-base-100/70 backdrop-blur-sm"
                                required
                            />
                        </div>

                        <div className="form-control space-y-2">
                            <label className="label">
                                <span className="label-text text-base-content/90">
                                    Password
                                </span>
                            </label>
                            <div className="relative">
                                <input
                                    type={showPassword ? "text" : "password"}
                                    placeholder="Password"
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    className="input input-bordered input-lg w-full focus:input-primary bg-base-100/70 pr-20 backdrop-blur-sm"
                                    required
                                />
                                <button
                                    type="button"
                                    className="absolute right-4 top-1/2 -translate-y-1/2 text-base-content/70 hover:text-base-content text-sm font-medium"
                                    onClick={() => setShowPassword(!showPassword)}
                                >
                                    {showPassword ? "Hide" : "Show"}
                                </button>
                            </div>
                        </div>

                        <div className="form-control space-y-2">
                            <label className="label">
                                <span className="label-text text-base-content/90">
                                    Confirm Password
                                </span>
                            </label>
                            <div className="relative">
                                <input
                                    type={showConfirmPassword ? "text" : "password"}
                                    placeholder="Confirm Password"
                                    value={passwordConfirm}
                                    onChange={(e) => setPasswordConfirm(e.target.value)}
                                    className="input input-bordered input-lg w-full focus:input-primary bg-base-100/70 pr-20 backdrop-blur-sm"
                                    required
                                />
                                <button
                                    type="button"
                                    className="absolute right-4 top-1/2 -translate-y-1/2 text-base-content/70 hover:text-base-content text-sm font-medium"
                                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                                >
                                    {showConfirmPassword ? "Hide" : "Show"}
                                </button>
                            </div>
                        </div>

                        {error && (
                            <div className="text-error text-sm text-center">{error}</div>
                        )}

                        <button
                            type="submit"
                            className="btn btn-primary btn-lg w-full mt-6"
                            disabled={loading}
                        >
                            {loading ? "Creating account..." : "Create Account"}
                        </button>
                    </form>

                    <div className="divider text-base-content/40 my-8">OR</div>

                    <div className="space-y-3">
                        <button
                            className="btn btn-primary btn-lg w-full"
                            onClick={() => navigate("/login")}
                        >
                            Login
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}
