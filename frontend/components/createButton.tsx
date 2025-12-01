"use client";

import Link from "next/link";
import Image from "next/image";

interface CreateButtonProps {
  href: string;
  label: string;
  icon?: string;
}

export default function CreateButton({
  href,
  label,
  icon = "/images/Plus.svg",
}: CreateButtonProps) {
  return (
    <Link href={href} className="flex-1 flex justify-end">
      <p
        className="flex flex-row items-center gap-2 p-1 rounded-full hover:cursor-pointer"
        aria-label={label}
      >
        <span className="text-[#162218] font-medium">{label}</span>
        <Image
          src={icon}
          alt={label + " Icon"}
          width={40}
          height={40}
          priority
        />
      </p>
    </Link>
  );
}
