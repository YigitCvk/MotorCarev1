import type { NextConfig } from 'next';

const nextConfig: NextConfig = {
  output: 'standalone',
  // API_BASE_URL: server-side only, used for the /api/* proxy rewrite.
  // NEXT_PUBLIC_API_BASE_URL: exposed to browser for direct API calls when needed.
  async rewrites() {
    const apiBase = process.env.API_BASE_URL ?? process.env.NEXT_PUBLIC_API_BASE_URL ?? 'http://localhost:5102';
    return [
      {
        source: '/api/:path*',
        destination: `${apiBase.replace(/\/$/, '')}/api/:path*`,
      },
    ];
  },
};

export default nextConfig;
