"use client";

import { useParamsStore } from "@/hooks/useParamsStore";
import React from "react";
import { FaCarAlt } from "react-icons/fa";

export default function Logo() {
  const reset = useParamsStore((status) => status.resetState);
  return (
    <div onClick={reset} className="cursor-pointer flex items-center gap-2 text-3xl font-semibold text-red-500">
      <FaCarAlt size={34} />
      <div>Auctions App</div>
    </div>
  );
}
