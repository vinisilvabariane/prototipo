import type { NextConfig } from "next";

const UPLOAD_LIMIT = "55mb";

const nextConfig: NextConfig = {
  cacheComponents: true,
  partialPrefetching: true,
  experimental: {
    serverActions: { bodySizeLimit: UPLOAD_LIMIT },
    proxyClientMaxBodySize: UPLOAD_LIMIT,
  },
  turbopack: {
    rules: {
      "*.css": {
        loaders: ["@tailwindcss/turbopack"],
        as: "*.css",
      },
    },
  },
};

export default nextConfig;
