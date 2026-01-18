import { startMockApiServer } from './mock-api-server';

try {
  const server = await startMockApiServer(5151);
  console.log('Mock API server started successfully');

  process.on('SIGTERM', async () => {
    console.log('Received SIGTERM, shutting down mock API server...');
    await server.close();
    process.exit(0);
  });

  process.on('SIGINT', async () => {
    console.log('Received SIGINT, shutting down mock API server...');
    await server.close();
    process.exit(0);
  });
} catch (error) {
  console.error('Failed to start mock API server:', error);
  process.exit(1);
}
