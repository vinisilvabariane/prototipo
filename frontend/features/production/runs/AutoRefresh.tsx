"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";

export default function AutoRefresh({ everySeconds = 30 }: { everySeconds?: number }) {
  const router = useRouter();

  useEffect(() => {
    const timer = setInterval(() => {
      if (document.visibilityState === "visible") router.refresh();
    }, everySeconds * 1000);

    return () => clearInterval(timer);
  }, [router, everySeconds]);

  return null;
}
