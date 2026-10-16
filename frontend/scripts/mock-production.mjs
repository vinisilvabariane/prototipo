const MAX_MACHINES = 5;
const MAX_QUANTITY = 1_000_000;
const HOUR = 3_600_000;
const POST_KINDS = ["standard", "machine", "quality"];
const DECORATION_KINDS = ["belt", "zone", "arrow", "machine", "caution", "rack", "table", "pillar"];

const truncateHour = (value) => Math.floor(new Date(value).getTime() / HOUR) * HOUR;
const iso = (ms) => new Date(ms).toISOString();

export function createProductionMock(users) {
  let counter = 2000;
  const next = () => ++counter;
  const now = () => Date.now();

  const people = [
    ...users.map((user) => ({ id: user.id, name: user.name, registration: user.registration, department: user.department ?? "" })),
    { id: 11, name: "Diego Rocha", registration: 2011, department: "producao" },
    { id: 12, name: "Elisa Prado", registration: 2012, department: "producao" },
    { id: 13, name: "Fabio Nunes", registration: 2013, department: "producao" },
    { id: 14, name: "Gabi Torres", registration: 2014, department: "producao" },
    { id: 15, name: "Hugo Mendes", registration: 2015, department: "qualidade" },
    { id: 16, name: "Iara Campos", registration: 2016, department: "producao" },
  ];
  const nameOf = (id) => people.find((person) => person.id === id)?.name ?? "";

  const lines = [
    { id: 1, name: "Linha 1", active: true, createdAt: iso(now() - 90 * 86_400_000) },
    { id: 2, name: "Linha 2", active: true, createdAt: iso(now() - 60 * 86_400_000) },
  ];
  const shifts = [
    { id: 1, name: "Turno 1", startTime: "06:00:00", endTime: "14:00:00", active: true },
    { id: 2, name: "Turno 2", startTime: "14:00:00", endTime: "22:00:00", active: true },
  ];
  const products = [
    { id: 1, code: "P-100", name: "Fone X", active: true },
    { id: 2, code: "P-200", name: "Caixa de som Y", active: true },
  ];

  const layouts = [];
  const runs = [];

  const makePost = (name, kind, x, y, extra = {}) => ({
    id: next(),
    name,
    kind,
    x,
    y,
    width: 210,
    height: 120,
    rotation: 0,
    isCritical: false,
    targetPerHour: null,
    operation: null,
    ...extra,
  });

  const makeDecoration = (kind, x, y, width, height, label = null) => ({ id: next(), kind, x, y, width, height, rotation: 0, label, locked: false });

  const seedLayout = (lineId, productId, shiftId) => {
    const layout = {
      id: next(),
      lineId,
      productId,
      shiftId,
      createdAt: iso(now() - 5 * 86_400_000),
      updatedAt: iso(now() - 86_400_000),
      updatedBy: 1,
      posts: [
        makePost("Alimentação", "standard", 60, 150, { isCritical: true, targetPerHour: 120, operation: "Posicionar a placa na esteira" }),
        makePost("Solda A", "machine", 330, 90, { targetPerHour: 60, operation: "Soldagem automática dos pontos" }),
        makePost("Solda B", "machine", 330, 270, { targetPerHour: 60, operation: "Soldagem automática dos pontos" }),
        makePost("Montagem", "standard", 600, 180, { isCritical: true, targetPerHour: 90, operation: "Montar carcaça e fixar parafusos" }),
        makePost("Inspeção final", "quality", 870, 180, { targetPerHour: 90, operation: "Conferir visual e teste funcional" }),
      ],
      decorations: [
        makeDecoration("belt", 30, 330, 1050, 60),
        makeDecoration("zone", 300, 30, 270, 420, "Células de solda"),
        makeDecoration("arrow", 540, 210, 60, 60),
        makeDecoration("caution", 600, 30, 240, 60, "Atenção"),
        makeDecoration("table", 870, 30, 240, 90),
      ],
    };
    layouts.push(layout);
    return layout;
  };

  seedLayout(1, 1, 1);
  const layout2 = seedLayout(2, 2, 1);

  const snapshotOf = (layout) => ({
    posts: layout.posts.map((post) => ({ ...post })),
    decorations: layout.decorations.map((decoration) => ({ kind: decoration.kind, x: decoration.x, y: decoration.y, width: decoration.width, height: decoration.height, rotation: decoration.rotation, label: decoration.label })),
  });

  const createRun = (layout, creator) => {
    const run = {
      id: next(),
      layoutId: layout.id,
      lineId: layout.lineId,
      productId: layout.productId,
      shiftId: layout.shiftId,
      status: "preparing",
      snapshot: snapshotOf(layout),
      createdAt: iso(now()),
      createdBy: creator,
      startedAt: null,
      startedBy: null,
      endedAt: null,
      endedBy: null,
      allocations: [],
      outputs: [],
    };
    runs.push(run);
    return run;
  };

  const allocate = (run, postId, userId, by, at = now()) => {
    const current = run.allocations.find((item) => item.postId === postId && item.releasedAt === null);
    if (current?.userId === userId) return false;
    if (current) current.releasedAt = at;
    run.allocations.push({ id: next(), postId, userId, allocatedAt: at, releasedAt: null, allocatedBy: by });
    return true;
  };

  const demo = createRun(layout2, 1);
  demo.createdAt = iso(now() - 3 * HOUR);
  const startAt = truncateHour(now() - 2 * HOUR);
  for (const [postName, userId] of [["Alimentação", 11], ["Solda A", 12], ["Solda B", 12], ["Montagem", 13], ["Inspeção final", 15]]) {
    allocate(demo, demo.snapshot.posts.find((post) => post.name === postName).id, userId, 1, startAt);
  }
  demo.status = "running";
  demo.startedAt = iso(startAt);
  demo.startedBy = 1;
  const quantities = { Alimentação: [110, 118], "Solda A": [54, 58], "Solda B": [48, 52], Montagem: [82, 88], "Inspeção final": [80, 85] };
  for (const post of demo.snapshot.posts) {
    quantities[post.name].forEach((quantity, index) => {
      const holder = demo.allocations.find((item) => item.postId === post.id);
      demo.outputs.push({ id: next(), postId: post.id, hourStart: startAt + index * HOUR, quantity, userId: holder.userId, recordedBy: 1, recordedAt: iso(startAt + (index + 1) * HOUR - 600_000) });
    });
  }

  const overlapMinutes = (from, to, windowFrom, windowTo) => Math.max(0, (Math.min(to, windowTo) - Math.max(from, windowFrom)) / 60_000);

  const productivity = (run) => {
    if (!run.startedAt) {
      return { posts: new Map(run.snapshot.posts.map((post) => [post.id, { occupiedMinutes: 0, produced: 0, expected: null, attainment: null }])), people: [], hours: [], total: 0 };
    }

    const from = new Date(run.startedAt).getTime();
    const to = run.endedAt ? new Date(run.endedAt).getTime() : now();

    const posts = new Map(
      run.snapshot.posts.map((post) => {
        const minutes = run.allocations.filter((item) => item.postId === post.id).reduce((sum, item) => sum + overlapMinutes(item.allocatedAt, item.releasedAt ?? to, from, to), 0);
        const produced = run.outputs.filter((item) => item.postId === post.id).reduce((sum, item) => sum + item.quantity, 0);
        const expected = post.targetPerHour !== null && minutes > 0 ? Math.round(((post.targetPerHour * minutes) / 60) * 100) / 100 : null;
        const attainment = expected ? Math.round((produced / expected) * 10_000) / 10_000 : null;
        return [post.id, { occupiedMinutes: Math.round(minutes * 10) / 10, produced, expected, attainment }];
      }),
    );

    const userIds = [...new Set(run.allocations.map((item) => item.userId))];
    const peopleRows = userIds
      .map((userId) => {
        const intervals = run.allocations
          .filter((item) => item.userId === userId)
          .map((item) => [Math.max(item.allocatedAt, from), Math.min(item.releasedAt ?? to, to)])
          .filter(([a, b]) => b > a)
          .sort((a, b) => a[0] - b[0]);
        let total = 0;
        let current = null;
        for (const [a, b] of intervals) {
          if (!current) current = [a, b];
          else if (a <= current[1]) current[1] = Math.max(current[1], b);
          else {
            total += current[1] - current[0];
            current = [a, b];
          }
        }
        if (current) total += current[1] - current[0];
        return { userId, minutes: Math.round((total / 60_000) * 10) / 10, produced: run.outputs.filter((item) => item.userId === userId).reduce((sum, item) => sum + item.quantity, 0) };
      })
      .sort((a, b) => b.produced - a.produced);

    const byHour = new Map();
    for (const output of run.outputs) byHour.set(output.hourStart, (byHour.get(output.hourStart) ?? 0) + output.quantity);
    const hours = [...byHour.entries()].sort((a, b) => a[0] - b[0]).map(([hourStart, produced]) => ({ hourStart: iso(hourStart), produced }));

    return { posts, people: peopleRows, hours, total: run.outputs.reduce((sum, item) => sum + item.quantity, 0) };
  };

  const nameFor = (collection, id, key = "name") => collection.find((item) => item.id === id)?.[key] ?? "";

  const runSummary = (run) => ({
    id: run.id,
    status: run.status,
    lineId: run.lineId,
    lineName: nameFor(lines, run.lineId),
    productCode: nameFor(products, run.productId, "code"),
    productName: nameFor(products, run.productId),
    shiftName: nameFor(shifts, run.shiftId),
    createdAt: run.createdAt,
    startedAt: run.startedAt,
    endedAt: run.endedAt,
    allocatedPosts: run.allocations.filter((item) => item.releasedAt === null).length,
    totalPosts: run.snapshot.posts.length,
    produced: run.outputs.reduce((sum, item) => sum + item.quantity, 0),
  });

  const runDetail = (run, perms) => {
    const report = productivity(run);
    const open = run.allocations.filter((item) => item.releasedAt === null);
    return {
      id: run.id,
      status: run.status,
      layoutId: run.layoutId,
      lineId: run.lineId,
      lineName: nameFor(lines, run.lineId),
      productCode: nameFor(products, run.productId, "code"),
      productName: nameFor(products, run.productId),
      shiftName: nameFor(shifts, run.shiftId),
      createdAt: run.createdAt,
      createdBy: nameOf(run.createdBy),
      startedAt: run.startedAt,
      startedBy: run.startedBy ? nameOf(run.startedBy) : null,
      endedAt: run.endedAt,
      endedBy: run.endedBy ? nameOf(run.endedBy) : null,
      posts: run.snapshot.posts.map((post) => {
        const holder = open.find((item) => item.postId === post.id);
        const metrics = report.posts.get(post.id);
        const person = holder ? people.find((item) => item.id === holder.userId) : null;
        return {
          ...post,
          allocation: holder ? { userId: holder.userId, userName: person.name, registration: person.registration, allocatedAt: iso(holder.allocatedAt) } : null,
          ...metrics,
        };
      }),
      decorations: run.snapshot.decorations,
      people: report.people.map((row) => ({
        userId: row.userId,
        userName: nameOf(row.userId),
        registration: people.find((item) => item.id === row.userId)?.registration ?? 0,
        minutes: row.minutes,
        produced: row.produced,
        openPosts: open.filter((item) => item.userId === row.userId).length,
      })),
      hours: report.hours,
      totalProduced: report.total,
      canSupervise: perms.supervise,
      canRecord: perms.record,
    };
  };

  const layoutSummary = (layout) => ({
    id: layout.id,
    lineId: layout.lineId,
    lineName: nameFor(lines, layout.lineId),
    productId: layout.productId,
    productCode: nameFor(products, layout.productId, "code"),
    productName: nameFor(products, layout.productId),
    shiftId: layout.shiftId,
    shiftName: nameFor(shifts, layout.shiftId),
    postCount: layout.posts.length,
    updatedAt: layout.updatedAt,
  });

  const layoutDetail = (layout, canManage) => ({
    ...layoutSummary(layout),
    posts: layout.posts,
    decorations: layout.decorations,
    updatedBy: nameOf(layout.updatedBy),
    canManage,
  });

  const kindOf = (run, postId) => run.snapshot.posts.find((post) => post.id === postId)?.kind;

  const allocationProblem = (run, postId, userId) => {
    if (run.status === "closed") return "A produção já foi encerrada.";
    const post = run.snapshot.posts.find((item) => item.id === postId);
    if (!post) return null;
    if (run.allocations.some((item) => item.releasedAt === null && item.postId === postId && item.userId === userId)) return undefined;
    const others = run.allocations.filter((item) => item.releasedAt === null && item.userId === userId && item.postId !== postId);
    if (post.kind === "machine") {
      if (others.some((item) => kindOf(run, item.postId) !== "machine")) return "A pessoa está em um posto comum e não pode acumular máquinas.";
      return others.length >= MAX_MACHINES ? `A pessoa já atingiu o limite de ${MAX_MACHINES} máquinas nesta produção.` : undefined;
    }
    return others.length > 0 ? "A pessoa já está em outro posto desta produção. Posto comum aceita apenas quem não está em nenhum outro." : undefined;
  };

  const busyElsewhere = (userId, exceptRunId) => runs.find((run) => run.id !== exceptRunId && run.status !== "closed" && run.allocations.some((item) => item.userId === userId && item.releasedAt === null));

  const lookupFor = (lineId, productId, shiftId) => {
    const line = lines.find((item) => item.id === lineId);
    const product = products.find((item) => item.id === productId);
    const shift = shifts.find((item) => item.id === shiftId);
    if (!line) return { status: 404, message: "Linha não encontrada." };
    if (!product) return { status: 404, message: "Produto não encontrado." };
    if (!shift) return { status: 404, message: "Turno não encontrado." };
    if (!line.active) return { status: 400, message: "A linha está desativada." };
    if (!product.active) return { status: 400, message: "O produto está desativado." };
    if (!shift.active) return { status: 400, message: "O turno está desativado." };
    return null;
  };

  const readLayoutBody = (layout, body) => {
    if (!Array.isArray(body?.posts) || !Array.isArray(body?.decorations)) return "Envie a lista de postos.";
    if (body.posts.length > 200) return "O layout pode ter no máximo 200 postos.";
    const names = new Set();
    for (const post of body.posts) {
      const name = String(post.name ?? "").trim();
      if (!name) return "Todo posto precisa de um nome.";
      if (names.has(name.toLowerCase())) return `O nome de posto "${name}" está repetido neste layout.`;
      names.add(name.toLowerCase());
      if (!POST_KINDS.includes(post.kind)) return "Tipo de posto inválido.";
      if (post.id > 0 && !layout.posts.some((item) => item.id === post.id)) return "Um dos postos enviados não pertence a este layout.";
      if (post.width < 40 || post.height < 40) return "A posição ou o tamanho de um posto é inválido.";
    }
    for (const decoration of body.decorations) {
      if (!DECORATION_KINDS.includes(decoration.kind)) return "Tipo de decoração inválido.";
      if (decoration.id > 0 && !layout.decorations.some((item) => item.id === decoration.id)) return "Uma das decorações enviadas não pertence a este layout.";
    }
    return null;
  };

  return function handle({ req, res, url, segments, actor, json, send, fail }) {
    const [, , , a, b, c, d, e] = segments;
    const full = actor !== 3;
    const perms = { manage: full, supervise: full, record: true };
    const denied = () => fail(res, 403, "Acesso negado", "Sua matrícula não tem permissão para esta operação.");
    const refuse = (message, status = 400, title = "Falha na operação") => fail(res, status, title, message);

    if (["lines", "shifts", "products"].includes(a)) {
      const table = { lines, shifts, products }[a];

      if (req.method === "GET") {
        const includeInactive = url.searchParams.get("includeInactive") === "true";
        return send(res, 200, table.filter((item) => includeInactive || item.active));
      }

      if (!perms.manage) return denied();

      const body = json ?? {};
      const validate = () => {
        if (a === "lines" && !String(body.name ?? "").trim()) return "Informe o nome da linha.";
        if (a === "shifts") {
          if (!String(body.name ?? "").trim()) return "Informe o nome do turno.";
          if (body.startTime === body.endTime) return "O início e o fim do turno não podem ser iguais.";
        }
        if (a === "products") {
          if (!String(body.code ?? "").trim()) return "Informe o código do produto.";
          if (!String(body.name ?? "").trim()) return "Informe o nome do produto.";
        }
        return null;
      };

      const problem = validate();
      if (problem) return refuse(problem);

      const keyOf = (item) => (a === "products" ? String(item.code).toUpperCase() : String(item.name).toLowerCase());
      const incomingKey = a === "products" ? String(body.code).trim().toUpperCase() : String(body.name).trim().toLowerCase();
      const clash = table.find((item) => keyOf(item) === incomingKey && String(item.id) !== b);

      if (req.method === "POST") {
        if (clash) return refuse(a === "products" ? `O código "${incomingKey}" já está em uso.` : `Já existe um cadastro com o nome "${String(body.name).trim()}".`);
        const created = a === "products" ? { id: next(), code: incomingKey, name: String(body.name).trim(), active: true } : a === "shifts" ? { id: next(), name: String(body.name).trim(), startTime: body.startTime, endTime: body.endTime, active: true } : { id: next(), name: String(body.name).trim(), active: true, createdAt: iso(now()) };
        table.push(created);
        return send(res, 201, created);
      }

      if (req.method === "PUT") {
        const item = table.find((candidate) => String(candidate.id) === b);
        if (!item) return refuse("Cadastro não encontrado.", 404);
        if (clash) return refuse(a === "products" ? `O código "${incomingKey}" já está em uso.` : `Já existe um cadastro com o nome "${String(body.name).trim()}".`);
        if (a === "products") Object.assign(item, { code: incomingKey, name: String(body.name).trim(), active: Boolean(body.active) });
        else if (a === "shifts") Object.assign(item, { name: String(body.name).trim(), startTime: body.startTime, endTime: body.endTime, active: Boolean(body.active) });
        else Object.assign(item, { name: String(body.name).trim(), active: Boolean(body.active) });
        return send(res, 204);
      }
    }

    if (a === "layouts") {
      if (req.method === "GET" && !b) {
        const lineId = Number(url.searchParams.get("lineId") ?? 0);
        const productId = Number(url.searchParams.get("productId") ?? 0);
        const shiftId = Number(url.searchParams.get("shiftId") ?? 0);
        const items = layouts
          .filter((layout) => (!lineId || layout.lineId === lineId) && (!productId || layout.productId === productId) && (!shiftId || layout.shiftId === shiftId))
          .map(layoutSummary)
          .sort((x, y) => x.lineName.localeCompare(y.lineName));
        return send(res, 200, { items, canManage: perms.manage });
      }

      if (req.method === "POST" && !b) {
        if (!perms.manage) return denied();
        const problem = lookupFor(json?.lineId, json?.productId, json?.shiftId);
        if (problem) return refuse(problem.message, problem.status);
        if (layouts.some((layout) => layout.lineId === json.lineId && layout.productId === json.productId && layout.shiftId === json.shiftId)) {
          return refuse("Já existe um layout para essa linha, produto e turno.");
        }
        const created = { id: next(), lineId: json.lineId, productId: json.productId, shiftId: json.shiftId, createdAt: iso(now()), updatedAt: iso(now()), updatedBy: actor, posts: [], decorations: [] };
        layouts.push(created);
        return send(res, 201, layoutSummary(created));
      }

      const layout = layouts.find((candidate) => String(candidate.id) === b);
      if (!layout) return refuse("Layout não encontrado.", 404, "Falha ao buscar layout");

      if (req.method === "GET" && !c) return send(res, 200, layoutDetail(layout, perms.manage));

      if (!perms.manage) return denied();

      if (req.method === "PUT" && !c) {
        const problem = readLayoutBody(layout, json);
        if (problem) return refuse(problem, 400, "Falha ao salvar layout");

        layout.posts = json.posts.map((post) => ({
          id: post.id > 0 ? post.id : next(),
          name: String(post.name).trim(),
          kind: post.kind,
          x: post.x,
          y: post.y,
          width: post.width,
          height: post.height,
          rotation: post.rotation ?? 0,
          isCritical: Boolean(post.isCritical),
          targetPerHour: post.targetPerHour ?? null,
          operation: post.operation ? String(post.operation).trim() : null,
        }));
        layout.decorations = json.decorations.map((decoration) => ({
          id: decoration.id > 0 ? decoration.id : next(),
          kind: decoration.kind,
          x: decoration.x,
          y: decoration.y,
          width: decoration.width,
          height: decoration.height,
          rotation: decoration.rotation ?? 0,
          label: decoration.label ? String(decoration.label).trim() : null,
          locked: Boolean(decoration.locked),
        }));
        layout.updatedAt = iso(now());
        layout.updatedBy = actor;
        return send(res, 204);
      }

      if (req.method === "DELETE" && !c) {
        layouts.splice(layouts.indexOf(layout), 1);
        return send(res, 204);
      }

      if (req.method === "POST" && c === "copy") {
        const targets = json?.targets ?? [];
        if (targets.length === 0) return refuse("Selecione ao menos um destino para a cópia.");
        if (targets.some((target) => target.lineId === layout.lineId && target.productId === layout.productId && target.shiftId === layout.shiftId)) return refuse("O destino da cópia não pode ser o próprio layout.");
        let created = 0;
        let updated = 0;
        for (const target of targets) {
          const problem = lookupFor(target.lineId, target.productId, target.shiftId);
          if (problem) return refuse(problem.message, problem.status);
          const copyContent = (to) => {
            to.posts = layout.posts.map((post) => ({ ...post, id: next() }));
            to.decorations = layout.decorations.map((decoration) => ({ ...decoration, id: next() }));
            to.updatedAt = iso(now());
            to.updatedBy = actor;
          };
          const existing = layouts.find((item) => item.lineId === target.lineId && item.productId === target.productId && item.shiftId === target.shiftId);
          if (existing) {
            copyContent(existing);
            updated++;
          } else {
            const fresh = { id: next(), ...target, createdAt: iso(now()), posts: [], decorations: [] };
            copyContent(fresh);
            layouts.push(fresh);
            created++;
          }
        }
        return send(res, 200, { created, updated });
      }
    }

    if (a === "runs") {
      if (req.method === "GET" && !b) {
        const status = url.searchParams.get("status");
        const lineId = Number(url.searchParams.get("lineId") ?? 0);
        const page = Number(url.searchParams.get("page") ?? 0);
        const size = Number(url.searchParams.get("pageSize") ?? 10);
        if (status && !["preparing", "running", "closed"].includes(status)) return refuse("Corpo da requisição inválido ou mal formatado.");
        const found = runs.filter((run) => (!status || run.status === status) && (!lineId || run.lineId === lineId)).sort((x, y) => y.createdAt.localeCompare(x.createdAt));
        return send(res, 200, { items: found.slice(page * size, page * size + size).map(runSummary), totalCount: found.length, page, pageSize: size, canSupervise: perms.supervise });
      }

      if (req.method === "POST" && !b) {
        if (!perms.supervise) return denied();
        const layout = layouts.find((item) => item.id === json?.layoutId);
        if (!layout) return refuse("Layout não encontrado.", 404, "Falha ao preparar a produção");
        const problem = lookupFor(layout.lineId, layout.productId, layout.shiftId);
        if (problem) return refuse(problem.message, problem.status);
        if (runs.some((run) => run.lineId === layout.lineId && run.status !== "closed")) return refuse(`A linha ${nameFor(lines, layout.lineId)} já tem uma produção em andamento ou em preparo.`, 400, "Falha ao preparar a produção");
        return send(res, 201, runSummary(createRun(layout, actor)));
      }

      const run = runs.find((candidate) => String(candidate.id) === b);
      if (!run) return refuse("Produção não encontrada.", 404, "Falha ao buscar produção");

      if (req.method === "GET" && !c) return send(res, 200, runDetail(run, perms));

      if (req.method === "POST" && c === "outputs") {
        const body = json ?? {};
        if (run.status !== "running") return refuse("A produção não está em andamento.");
        if (!run.snapshot.posts.some((post) => post.id === body.postId)) return refuse("O posto não existe nesta produção.", 404);
        if (!Number.isInteger(body.quantity) || body.quantity < 0 || body.quantity > MAX_QUANTITY) return refuse("A quantidade precisa ser um número inteiro entre 0 e 1.000.000.");
        const slot = truncateHour(body.hourStart ?? now());
        if (slot > truncateHour(now())) return refuse("Não é possível lançar produção de uma hora que ainda não começou.");
        if (slot < truncateHour(run.startedAt)) return refuse("Não é possível lançar produção de uma hora anterior ao início da produção.");
        const holder = run.allocations
          .filter((item) => item.postId === body.postId)
          .map((item) => ({ item, overlap: overlapMinutes(item.allocatedAt, item.releasedAt ?? slot + HOUR, slot, slot + HOUR) }))
          .filter((entry) => entry.overlap > 0)
          .sort((x, y) => y.overlap - x.overlap)[0]?.item;
        const existing = run.outputs.find((item) => item.postId === body.postId && item.hourStart === slot);
        if (existing) Object.assign(existing, { quantity: body.quantity, userId: holder?.userId ?? null, recordedBy: actor, recordedAt: iso(now()) });
        else run.outputs.push({ id: next(), postId: body.postId, hourStart: slot, quantity: body.quantity, userId: holder?.userId ?? null, recordedBy: actor, recordedAt: iso(now()) });
        return send(res, 204);
      }

      if (!perms.supervise) return denied();

      if (req.method === "GET" && c === "candidates") {
        const term = (url.searchParams.get("search") ?? "").trim().toLowerCase();
        const items = people
          .filter((person) => !term || person.name.toLowerCase().includes(term) || String(person.registration).includes(term))
          .sort((x, y) => x.name.localeCompare(y.name))
          .map((person) => {
            const mine = run.allocations.filter((item) => item.releasedAt === null && item.userId === person.id);
            const busy = busyElsewhere(person.id, run.id);
            return {
              id: person.id,
              name: person.name,
              registration: person.registration,
              department: person.department,
              machinePosts: mine.filter((item) => kindOf(run, item.postId) === "machine").length,
              holdsCommonPost: mine.some((item) => kindOf(run, item.postId) !== "machine"),
              busyLine: busy ? nameFor(lines, busy.lineId) : null,
            };
          });
        return send(res, 200, items);
      }

      if (req.method === "POST" && c === "posts" && e === "allocation") {
        const postId = Number(d);
        const userId = json?.userId;
        if (!people.some((person) => person.id === userId)) return refuse("A pessoa não existe ou está desativada.", 404);
        const problem = allocationProblem(run, postId, userId);
        if (problem === null) return refuse("O posto não existe nesta produção.", 404);
        if (problem) return refuse(problem);
        const busy = busyElsewhere(userId, run.id);
        if (busy) return refuse(`${nameOf(userId)} já está alocado(a) na produção da linha ${nameFor(lines, busy.lineId)}.`);
        allocate(run, postId, userId, actor);
        return send(res, 204);
      }

      if (req.method === "DELETE" && c === "posts" && e === "allocation") {
        if (run.status === "closed") return refuse("A produção já foi encerrada.");
        const current = run.allocations.find((item) => item.postId === Number(d) && item.releasedAt === null);
        if (current) current.releasedAt = now();
        return send(res, 204);
      }

      if (req.method === "DELETE" && c === "allocations") {
        if (run.status === "closed") return refuse("A produção já foi encerrada.");
        for (const item of run.allocations) if (item.releasedAt === null) item.releasedAt = now();
        return send(res, 204);
      }

      if (req.method === "POST" && c === "allocations" && d === "copy-previous") {
        if (run.status === "closed") return refuse("A produção já foi encerrada.");
        const previous = runs.filter((item) => item.layoutId === run.layoutId && item.status === "closed").sort((x, y) => y.endedAt.localeCompare(x.endedAt))[0];
        if (!previous) return refuse("Não há uma produção anterior encerrada para este layout.", 404);
        const endedAt = new Date(previous.endedAt).getTime();
        let allocated = 0;
        let skipped = 0;
        for (const entry of previous.allocations.filter((item) => item.releasedAt === endedAt)) {
          const previousPost = previous.snapshot.posts.find((post) => post.id === entry.postId);
          const post = run.snapshot.posts.find((candidate) => candidate.name.toLowerCase() === previousPost?.name.toLowerCase());
          if (!post || busyElsewhere(entry.userId, run.id) || allocationProblem(run, post.id, entry.userId)) {
            skipped++;
            continue;
          }
          if (allocate(run, post.id, entry.userId, actor)) allocated++;
        }
        return send(res, 200, { allocated, skipped });
      }

      if (req.method === "POST" && c === "start") {
        if (run.status !== "preparing") return refuse("A produção já foi iniciada ou encerrada.");
        const open = run.allocations.filter((item) => item.releasedAt === null);
        if (open.length === 0) return refuse("Aloque ao menos uma pessoa antes de iniciar a produção.");
        const missing = run.snapshot.posts.filter((post) => post.isCritical && !open.some((item) => item.postId === post.id)).map((post) => post.name);
        if (missing.length > 0) return refuse(`Há postos críticos sem ninguém: ${missing.join(", ")}.`);
        run.status = "running";
        run.startedAt = iso(now());
        run.startedBy = actor;
        return send(res, 204);
      }

      if (req.method === "POST" && c === "close") {
        if (run.status !== "running") return refuse("A produção não está em andamento.");
        const at = now();
        for (const item of run.allocations) if (item.releasedAt === null) item.releasedAt = at;
        run.status = "closed";
        run.endedAt = iso(at);
        run.endedBy = actor;
        return send(res, 204);
      }

      if (req.method === "DELETE" && !c) {
        if (run.status !== "preparing") return refuse("Só dá para cancelar uma produção que ainda não foi iniciada.");
        runs.splice(runs.indexOf(run), 1);
        return send(res, 204);
      }
    }

    return fail(res, 404, "Rota inexistente", url.pathname);
  };
}
