import { NextRequest, NextResponse } from "next/server";

function decodeJwt(token: string) {
  const payload = token.split(".")[1];
  return JSON.parse(Buffer.from(payload, "base64").toString());
}

export function middleware(req: NextRequest) {
  const path = req.nextUrl.pathname;

  // Public routes
  const publicRoutes = ["/login", "/register"];
  const isPublic = publicRoutes.some((route) => path.startsWith(route));

  // Static files
  if (path.startsWith("/_next") || path.startsWith("/static") || path.includes(".")) {
    return NextResponse.next();
  }

  // Authentication
  const token = req.cookies.get("jwt");
  if (!token && !isPublic) {
    return NextResponse.redirect(new URL("/login", req.url));
  }

  if (token && isPublic) {
    return NextResponse.redirect(new URL("/auctions", req.url));
  }

  // If public, skip role checks
  if (isPublic) {
    return NextResponse.next();
  }

  // Decode JWT to get role
  const payload = decodeJwt(token!.value);
  const userRole = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
  console.log(userRole);

  // Role + allowed routes map
  const rolePermissions: Record<string, string[]> = {
    "/products": ["supplier", "admin"],
    "/auctions/create": ["auctioneer"],
    "/auctions/delete": ["auctioneer"],
  };

  // Check if path matches a protected prefix
  for (const route in rolePermissions) {
    if (path.startsWith(route)) {
      const allowedRoles = rolePermissions[route];
      if (!allowedRoles.includes(userRole) && userRole !== "admin") {
        const deniedPath = req.nextUrl.pathname;
        const url = new URL("/auctions", req.url);
        url.searchParams.set(
          "message",
          `Je hebt geen toegang tot de pagina: ${deniedPath}`
        );
        return NextResponse.redirect(url);
      }
    }
  }

  return NextResponse.next();
}
