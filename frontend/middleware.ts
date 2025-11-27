import { NextRequest, NextResponse } from "next/server";
export function middleware(req: NextRequest) {
  const token = req.cookies.get("jwt");
  const path = req.nextUrl.pathname;
  const publicRoutes = ["/login", "/register"];
  const isPublic = publicRoutes.some((route) => path.startsWith(route));

  if (path.startsWith("/_next") || path.startsWith("/static") || path.includes(".")){
    return NextResponse.next();
  }

  if (!token && !isPublic) {
    return NextResponse.redirect(new URL("/login", req.url));
  }

  if (token && isPublic) {
    return NextResponse.redirect(new URL("/auctions", req.url))
  }
  
  return NextResponse.next();
}
