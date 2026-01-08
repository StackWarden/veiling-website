import { Suspense } from "react";
import Login from "@/components/auth/login";

export default function Logins() {
  return (
    <Suspense fallback={null}>
      <Login />
    </Suspense>
  );
}
