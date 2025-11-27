import useAuth from "../hooks/useAuth";

type RoleGateProps = {
  allow: string[];
  fallback?: React.ReactNode;
  children: React.ReactNode;
};

export function RoleGate({ allow, fallback = null, children }: RoleGateProps) {
  const { role, status } = useAuth();

  // if role admin see everything voodoo type shit
  if (role === "admin") return <>{children}</>;
  
  if (status === "loading") return null;
  if (!role || !allow.includes(role)) return fallback;

  return <>{children}</>;
}
