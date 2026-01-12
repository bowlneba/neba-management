import { createServer, IncomingMessage, ServerResponse } from 'http';

const MOCK_DOCUMENT_HTML = `
<div>
  <h1 id="section-10.3-annual-meeting">Section 10.3 Annual Meeting</h1>
  <p>The annual meeting shall be held...</p>
  <p>See also <a href="#established-election-cycle">established election cycle</a> and <a href="#quorum-provisions">quorum provisions</a>.</p>
  <p>For more details, refer to <a href="#section-12.1-amendments">amendments</a> and <a href="#article-vii-hall-of-fame-committee">Hall of Fame Committee</a>.</p>

  <h2 id="article-vii-hall-of-fame-committee">ARTICLE VII - HALL OF FAME COMMITTEE</h2>
  <p>The Hall of Fame Committee shall...</p>
  <p>Related: <a href="#section-10.3-annual-meeting">Annual Meeting</a></p>

  <h2 id="established-election-cycle">Established Election Cycle</h2>
  <p>Elections shall be conducted on a regular cycle...</p>

  <h2 id="quorum-provisions">Quorum Provisions</h2>
  <p>A quorum shall consist of...</p>

  <h2 id="section-12.1-amendments">Section 12.1 Amendments</h2>
  <p>These bylaws may be amended...</p>

  <!-- Link to another document -->
  <p>See <a href="/tournaments/rules">Tournament Rules</a> for competition guidelines.</p>
</div>
`;

export function startMockApiServer(port: number = 5150): Promise<{ close: () => Promise<void> }> {
  return new Promise((resolve) => {
    const server = createServer((req: IncomingMessage, res: ServerResponse) => {
      // Enable CORS
      res.setHeader('Access-Control-Allow-Origin', '*');
      res.setHeader('Access-Control-Allow-Methods', 'GET, OPTIONS');
      res.setHeader('Access-Control-Allow-Headers', 'Content-Type');

      if (req.method === 'OPTIONS') {
        res.writeHead(200);
        res.end();
        return;
      }

      // Handle /bylaws endpoint
      if (req.url === '/bylaws' && req.method === 'GET') {
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({
          content: MOCK_DOCUMENT_HTML,
          metadata: {
            LastUpdatedUtc: '2024-01-01T00:00:00Z',
            LastUpdatedBy: 'Test User'
          }
        }));
        return;
      }

      // Handle /bylaws/refresh/status endpoint
      if (req.url === '/bylaws/refresh/status' && req.method === 'GET') {
        res.writeHead(200, { 'Content-Type': 'text/event-stream' });
        res.end('data: {"status":"Completed"}\n\n');
        return;
      }

      // Default 404 response
      res.writeHead(404, { 'Content-Type': 'application/json' });
      res.end(JSON.stringify({ error: 'Not Found' }));
    });

    server.listen(port, () => {
      console.log(`Mock API server listening on http://localhost:${port}`);
      resolve({
        close: () => {
          return new Promise((resolveClose) => {
            server.close(() => {
              console.log('Mock API server closed');
              resolveClose();
            });
          });
        }
      });
    });
  });
}
