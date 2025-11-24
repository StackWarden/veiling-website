import useAuth from "../hooks/useAuth";

type RoleGateProps = {
  allow: string[];
  children: React.ReactNode;
};

export default function RoleGate({ allow, children }: RoleGateProps) {
  const { role, status } = useAuth();

  if (status === "loading") return null;
  if (!role || !allow.includes(role)) return null;

  return <>{children}</>;
}
