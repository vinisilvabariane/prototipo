"use client";

import { useEffect } from "react";
import ErrorAlert from "@/components/ui/ErrorAlert";
import { failure } from "@/lib/result";

export default function DashboardError({ error, retry }: { error: Error & { digest?: string }; retry: () => void }) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return <ErrorAlert error={failure("unexpected").error} onRetry={retry} />;
}
