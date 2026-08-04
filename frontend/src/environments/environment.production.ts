export const environment = {
  production: true,
  // Served behind the frontend's nginx, which reverse-proxies /api/* to the backend container
  // (see frontend/nginx.conf) — same-origin, so no CORS and no hardcoded backend host/port.
  apiUrl: '/api',
};
