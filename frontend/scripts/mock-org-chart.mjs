const MAX_DEPTH = 12;

export function createOrgChartMock(users, departments) {
  let counter = 500;
  const positions = [];

  const add = (title, departmentId, parentId = null, holderId = null) => {
    const position = { id: ++counter, title, departmentId, parentId, holderId };
    positions.push(position);
    return position;
  };

  const ti = departments.find((department) => department.name === "ti")?.id ?? departments[0].id;
  const rh = departments.find((department) => department.name === "rh")?.id ?? departments[0].id;
  const quality = departments.find((department) => department.name === "qualidade")?.id ?? departments[0].id;

  const director = add("Diretor industrial", ti, null, users[0].id);
  const production = add("Gerente de produção", rh, director.id, users[1].id);
  const qualityManager = add("Gerente de qualidade", quality, director.id, users[2].id);
  const it = add("Gerente de TI", ti, director.id);
  add("Supervisor da linha 1", rh, production.id);
  add("Supervisor da linha 2", rh, production.id);
  add("Analista de qualidade", quality, qualityManager.id);
  add("Analista de sistemas", ti, it.id);
  add("Coordenador de RH", rh, null);

  const byId = (id) => positions.find((position) => position.id === id);
  const person = (id) => users.find((user) => user.id === id);
  const departmentName = (id) => departments.find((department) => department.id === id)?.name ?? "";

  const respond = (position) => {
    const holder = position.holderId ? person(position.holderId) : null;
    return {
      id: position.id,
      title: position.title,
      parentId: position.parentId,
      departmentId: position.departmentId,
      department: departmentName(position.departmentId),
      holder: holder ? { userId: holder.id, name: holder.name, registration: holder.registration, email: holder.email, active: true } : null,
    };
  };

  const depthOf = (id) => {
    let depth = 0;
    let current = id;
    while (current !== null && byId(current) && depth <= positions.length) {
      depth++;
      current = byId(current).parentId;
    }
    return depth;
  };

  const heightOf = (id, guard = 0) =>
    guard > positions.length ? 0 : 1 + Math.max(0, ...positions.filter((position) => position.parentId === id).map((child) => heightOf(child.id, guard + 1)));

  const selfOrBelow = (ancestorId, candidateId) => {
    let current = candidateId;
    let steps = 0;
    while (current !== null && current !== undefined && steps++ <= positions.length) {
      if (current === ancestorId) return true;
      current = byId(current)?.parentId ?? null;
    }
    return false;
  };

  return function handle({ req, res, url, segments, actor, json, send, fail }) {
    const [, , , a, id, leaf] = segments;
    const canManage = actor !== 3;
    const denied = () => fail(res, 403, "Acesso negado", "Sua matrícula não tem permissão para esta operação.");
    const taken = (userId, exceptId) => positions.find((position) => position.holderId === userId && position.id !== exceptId);

    if (req.method === "GET" && !a) {
      const sorted = [...positions].sort((x, y) => x.title.localeCompare(y.title) || x.id - y.id);
      return send(res, 200, { positions: sorted.map(respond), canManage });
    }

    if (!canManage) return denied();

    if (req.method === "GET" && a === "assignable-users") {
      const occupied = new Set(positions.map((position) => position.holderId));
      return send(
        res,
        200,
        users.filter((user) => !occupied.has(user.id)).map((user) => ({ id: user.id, registration: user.registration, name: user.name, department: user.department })),
      );
    }

    if (a !== "positions") return fail(res, 404, "Rota inexistente", url.pathname);

    if (req.method === "POST" && !id) {
      const title = String(json.title ?? "").trim();
      if (!title) return fail(res, 400, "Falha ao criar cargo", "Informe o nome do cargo.");
      if (!departments.some((department) => department.id === json.departmentId)) return fail(res, 404, "Falha ao criar cargo", "Departamento não encontrado.");
      if (json.parentId && !byId(json.parentId)) return fail(res, 404, "Falha ao criar cargo", "O cargo superior não foi encontrado.");
      if (json.parentId && depthOf(json.parentId) + 1 > MAX_DEPTH) return fail(res, 400, "Falha ao criar cargo", `A hierarquia não pode passar de ${MAX_DEPTH} níveis.`);
      if (json.holderId) {
        if (!person(json.holderId)) return fail(res, 404, "Falha ao criar cargo", "O colaborador não existe ou está desativado.");
        const other = taken(json.holderId);
        if (other) return fail(res, 400, "Falha ao criar cargo", `Este colaborador já ocupa o cargo "${other.title}".`);
      }
      const position = add(title, json.departmentId, json.parentId ?? null, json.holderId ?? null);
      return send(res, 201, respond(position));
    }

    const position = byId(Number(id));
    if (!position) return fail(res, 404, "Falha na operação", "Cargo não encontrado.");

    if (req.method === "PUT" && !leaf) {
      const title = String(json.title ?? "").trim();
      if (!title) return fail(res, 400, "Falha ao atualizar cargo", "Informe o nome do cargo.");
      if (!departments.some((department) => department.id === json.departmentId)) return fail(res, 404, "Falha ao atualizar cargo", "Departamento não encontrado.");
      position.title = title;
      position.departmentId = json.departmentId;
      return send(res, 204);
    }

    if (req.method === "PUT" && leaf === "holder") {
      if (json.userId) {
        if (!person(json.userId)) return fail(res, 404, "Falha ao definir o ocupante do cargo", "O colaborador não existe ou está desativado.");
        const other = taken(json.userId, position.id);
        if (other) return fail(res, 400, "Falha ao definir o ocupante do cargo", `Este colaborador já ocupa o cargo "${other.title}".`);
      }
      position.holderId = json.userId ?? null;
      return send(res, 204);
    }

    if (req.method === "PUT" && leaf === "parent") {
      const parentId = json.parentId ?? null;
      if (parentId !== null) {
        if (!byId(parentId)) return fail(res, 404, "Falha ao mover cargo", "O cargo superior não foi encontrado.");
        if (selfOrBelow(position.id, parentId)) return fail(res, 400, "Falha ao mover cargo", "Um cargo não pode ficar abaixo dele mesmo nem de um cargo subordinado a ele.");
        if (depthOf(parentId) + heightOf(position.id) > MAX_DEPTH) return fail(res, 400, "Falha ao mover cargo", `A hierarquia não pode passar de ${MAX_DEPTH} níveis.`);
      }
      position.parentId = parentId;
      return send(res, 204);
    }

    if (req.method === "DELETE" && !leaf) {
      if (positions.some((other) => other.parentId === position.id)) {
        return fail(res, 400, "Falha ao excluir cargo", "Não é possível excluir: o cargo tem subordinados. Mova ou exclua os cargos abaixo dele primeiro.");
      }
      positions.splice(positions.indexOf(position), 1);
      return send(res, 204);
    }

    return fail(res, 404, "Rota inexistente", url.pathname);
  };
}
