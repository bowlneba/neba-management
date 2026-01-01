import { createServer, IncomingMessage, Server, ServerResponse } from 'node:http';

const summaries = [
  {
    bowlerId: '01ARZ3NDEKTSV4RRFFQ69G5FAV',
    bowlerName: 'Mike Lichstein',
    titleCount: 32,
    hallOfFame: true,
  },
  {
    bowlerId: '01ARZ3NDEKTSV4RRFFQ69G5FBA',
    bowlerName: 'Alex Aguiar',
    titleCount: 29,
    hallOfFame: true,
  },
  {
    bowlerId: '01ARZ3NDEKTSV4RRFFQ69G5FBB',
    bowlerName: 'Bill Webb',
    titleCount: 22,
    hallOfFame: true,
  },
];

const titles = [
  {
    bowlerId: '01ARZ3NDEKTSV4RRFFQ69G5FAV',
    bowlerName: 'Mike Lichstein',
    tournamentMonth: 3,
    tournamentYear: 2024,
    tournamentType: 'Regular',
  },
  {
    bowlerId: '01ARZ3NDEKTSV4RRFFQ69G5FBA',
    bowlerName: 'Alex Aguiar',
    tournamentMonth: 1,
    tournamentYear: 2024,
    tournamentType: 'Masters',
  },
  {
    bowlerId: '01ARZ3NDEKTSV4RRFFQ69G5FBB',
    bowlerName: 'Bill Webb',
    tournamentMonth: 12,
    tournamentYear: 2023,
    tournamentType: 'Regular',
  },
];

const bowlerTitles: Record<string, unknown> = {
  '01ARZ3NDEKTSV4RRFFQ69G5FAV': {
    bowlerId: '01ARZ3NDEKTSV4RRFFQ69G5FAV',
    bowlerName: 'Mike Lichstein',
    hallOfFame: true,
    titles: [
      { month: 3, year: 2024, tournamentType: 'Regular' },
      { month: 1, year: 2024, tournamentType: 'Masters' },
    ],
  },
  '01ARZ3NDEKTSV4RRFFQ69G5FBA': {
    bowlerId: '01ARZ3NDEKTSV4RRFFQ69G5FBA',
    bowlerName: 'Alex Aguiar',
    hallOfFame: true,
    titles: [{ month: 2, year: 2024, tournamentType: 'Regular' }],
  },
  '01ARZ3NDEKTSV4RRFFQ69G5FBB': {
    bowlerId: '01ARZ3NDEKTSV4RRFFQ69G5FBB',
    bowlerName: 'Bill Webb',
    hallOfFame: true,
    titles: [{ month: 1, year: 2024, tournamentType: 'Regular' }],
  },
};

function json(res: ServerResponse, status: number, body: unknown) {
  res.statusCode = status;
  res.setHeader('Content-Type', 'application/json');
  res.end(JSON.stringify(body));
}

function handleRequest(req: IncomingMessage, res: ServerResponse) {
  const url = new URL(req.url ?? '/', 'http://localhost');
  const pathname = url.pathname;

  if (req.method !== 'GET' || !pathname) {
    json(res, 405, { error: 'Method not allowed' });
    return;
  }

  if (pathname === '/titles/summary') {
    json(res, 200, { items: summaries, totalItems: summaries.length });
    return;
  }

  if (pathname === '/titles') {
    json(res, 200, { items: titles, totalItems: titles.length });
    return;
  }

  const bowlerTitlesMatch = RegExp(/^\/bowlers\/(.+?)\/titles$/).exec(pathname);
  if (bowlerTitlesMatch) {
    const bowlerId = bowlerTitlesMatch[1];
    const payload = bowlerTitles[bowlerId];

    if (!payload) {
      json(res, 404, { error: 'Bowler not found' });
      return;
    }

    json(res, 200, { data: payload });
    return;
  }

  json(res, 404, { error: 'Not found' });
}

export function startMockApiServer(port = 5055): Promise<Server> {
  return new Promise((resolve) => {
    const server = createServer(handleRequest);
    server.listen(port, () => resolve(server));
  });
}

export function stopMockApiServer(server: Server): Promise<void> {
  return new Promise((resolve, reject) => {
    server.close((err) => {
      if (err) {
        reject(err);
        return;
      }
      resolve();
    });
  });
}
