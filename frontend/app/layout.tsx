import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Frontend",
  description: "React NextJS frontend",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <head>
        <link
          href="https://fonts.cdnfonts.com/css/sansation"
          rel="stylesheet"
        />
      </head>

      {/* Apply Sansation globally */}
      <body className="font-sansation antialiased">
        {children}
      </body>
    </html>
  );
}
