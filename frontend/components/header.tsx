"use client";

import { useEffect, useState } from "react";
import { usePathname } from "next/navigation";
import Image from "next/image";
import LogoutButton from "./logoutButton";

export default function Header() {
    const pathname = usePathname();

    // Dit wordt gebruikt om in de pagina de Role/User in te vullen
    const [user, setUser] = useState<{ name: string; role: string } | null>(null);

    // Helper functie die de eerste letter een hoofdletter maakt
    const cap = (str?: string) =>
        str ? str.charAt(0).toUpperCase() + str.slice(1) : "";

    // Controlleerd de path van de url en als dit overeen komt met de gegeven path return true
    const isActive = (path: string) => pathname.startsWith(path);

    useEffect(() => {
        const fetchUser = async () => {
            try {
                const res = await fetch(
                    `${process.env.NEXT_PUBLIC_API_URL}/auth/info`,
                    {
                        credentials: "include",
                    }
                );

                if (!res.ok) {
                    return;
                }

                const data = await res.json();

                setUser({
                    name: cap(data.name),
                    role: cap(data.role),
                });
            } catch (err) {
                console.error("Failed to fetch user info", err);
            }
        };

        fetchUser();
    }, []);

    return (
        <header className="w-full">
            <div className="w-full px-[5em] py-4 flex items-center justify-between">

                {/* Logo */}
                <div className="flex items-center">
                    <div className="bg-[#0f1c14] rounded-full p-1 flex items-center">
                        <Image
                            src="/logo.png"
                            alt="logo"
                            width={48}
                            height={48}
                            className="w-12 h-12"
                        />
                    </div>
                </div>

                {/* Navigation */}
                <nav className="flex items-center gap-8 text-base font-semibold">

                    <LogoutButton />

                    <a
                        href="/products"
                        className={
                            // Als de current path overeenkomt wordt deze underlined
                            isActive("/products")
                                ? "underline underline-offset-4"
                                : "hover:text-gray-600 transition"
                        }
                    >
                        Products
                    </a>

                    <a
                        href="/auctions"
                        className={
                            isActive("/auctions")
                                ? "underline underline-offset-4"
                                : "hover:text-gray-600 transition"
                        }
                    >
                        Auctions
                    </a>

                    <a
                        href="/notifications"
                        className={
                            isActive("/notifications")
                                ? "underline underline-offset-4"
                                : "hover:text-gray-600 transition"
                        }
                    >
                        Messages
                    </a>

                    {/* User info */}
                    <div className="flex p-2.5 justify-center items-center gap-2.5 rounded-lg bg-[#162218] text-white">
                        <span className="font-semibold">
                            {user?.role ?? "Loading"}:
                        </span>
                        <span className="font-semibold">
                            {user?.name ?? "..."}
                        </span>
                    </div>
                </nav>

            </div>
        </header>
    );
}
