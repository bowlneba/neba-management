import { startMockApiServer } from './mock-api-server';

// Start the mock API server on port 5151 (not 5150, to avoid conflict with Docker API)
startMockApiServer(5151).then((server) => {
  console.log('Mock API server started successfully');

  // Keep the process running
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
}).catch((error) => {
  console.error('Failed to start mock API server:', error);
  process.exit(1);
});
