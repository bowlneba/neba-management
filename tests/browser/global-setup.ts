import http from 'node:http';
import { startMockApiServer, stopMockApiServer } from './mocks/mock-api-server';
import type { Server } from 'node:http';

export default async function globalSetup() {
  const port = Number(process.env.MOCK_API_PORT ?? '5055');
  try {
    const server: Server = await startMockApiServer(port);

    return async () => {
      await stopMockApiServer(server);
    };
  }
  catch (error: unknown) {
    const err = error as NodeJS.ErrnoException;
    if (err.code !== 'EADDRINUSE') {
      throw err;
    }

    // If the port is in use, verify a server is actually responding; otherwise surface the error.
    await assertServerHealthy(port);
    // No teardown because we did not create the server.
    return async () => {};
  }
}

async function assertServerHealthy(port: number) {
  await new Promise<void>((resolve, reject) => {
    const req = http.get({ hostname: '127.0.0.1', port, path: '/titles/summary', timeout: 1500 }, (res) => {
      // Any 2xx response is fine for basic liveness
      if (res.statusCode && res.statusCode >= 200 && res.statusCode < 300) {
        resolve();
      }
      else {
        reject(new Error(`Port ${port} is in use but mock API health check failed with status ${res.statusCode}`));
      }
    });

    req.on('error', reject);
    req.on('timeout', () => {
      req.destroy(new Error(`Port ${port} is in use but did not respond to mock API health check`));
    });
  });
}
