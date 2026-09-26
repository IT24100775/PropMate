import { useState } from "react";
import { AuthContext } from "./authContext";
import * as authService from "../services/authService";

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const savedUser = localStorage.getItem("propmate_user");

    return savedUser ? JSON.parse(savedUser) : null;
  });

  const login = async (email, password) => {
    const data = await authService.login(email, password);

    const authenticatedUser = {
      userId: data.userId,
      email: data.email,
      role: data.role,
      token: data.token,
    };

    localStorage.setItem(
      "propmate_user",
      JSON.stringify(authenticatedUser)
    );

    setUser(authenticatedUser);

    return authenticatedUser;
  };

  const register = async (
    firstName,
    lastName,
    email,
    password
  ) => {
    const data = await authService.register(
      firstName,
      lastName,
      email,
      password
    );

    const authenticatedUser = {
      userId: data.userId,
      email: data.email,
      role: data.role,
      token: data.token,
    };

    localStorage.setItem(
      "propmate_user",
      JSON.stringify(authenticatedUser)
    );

    setUser(authenticatedUser);

    return authenticatedUser;
  };

  const logout = () => {
    localStorage.removeItem("propmate_user");
    setUser(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        login,
        register,
        logout,
        isAuthenticated: !!user,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}