const HOUR_MS = 3_600_000;
const MAX_FORMULA = 1000;
const MAX_SET_NAME = 60;
const iso = (ms) => new Date(ms).toISOString();
const truncateHour = (ms) => Math.floor(ms / HOUR_MS) * HOUR_MS;

const FUNCTIONS = {
  min: [1, 16],
  max: [1, 16],
  abs: [1, 1],
  clamp: [3, 3],
  if: [3, 3],
  round: [1, 2],
};

const FUNCTION_DOCS = [
  { name: "min", signature: "min(a, b, ...)", description: "Menor valor da lista." },
  { name: "max", signature: "max(a, b, ...)", description: "Maior valor da lista." },
  { name: "abs", signature: "abs(x)", description: "Valor absoluto." },
  { name: "clamp", signature: "clamp(x, mínimo, máximo)", description: "Mantém x entre o mínimo e o máximo." },
  { name: "if", signature: "if(condição, então, senão)", description: "Se a condição for verdadeira (≠ 0), devolve o então; senão, o senão. Compare com <, >, <=, >=, ==, !=." },
  { name: "round", signature: "round(x, casas)", description: "Arredonda x para o número de casas." },
];

class FormulaError extends Error {
  constructor(message, position) {
    super(message);
    this.position = position;
  }
}

function parse(source) {
  if (!source || !source.trim()) return { error: { message: "Escreva a fórmula.", position: 0 } };
  if (source.length > MAX_FORMULA) return { error: { message: `A fórmula não pode passar de ${MAX_FORMULA} caracteres.`, position: MAX_FORMULA } };

  let index = 0;
  const identifiers = new Set();
  const skip = () => {
    while (index < source.length && /\s/.test(source[index])) index++;
  };
  const match = (text) => source.startsWith(text, index);

  function expression(depth) {
    if (depth > 40) throw new FormulaError("A fórmula tem parênteses demais aninhados.", index);
    const left = additive(depth);
    skip();
    for (const op of ["<=", ">=", "==", "!=", "<", ">"]) {
      if (!match(op)) continue;
      index += op.length;
      const right = additive(depth);
      return (vars) => {
        const a = left(vars);
        const b = right(vars);
        const result = { "<": a < b, ">": a > b, "<=": a <= b, ">=": a >= b, "==": Math.abs(a - b) < 1e-9, "!=": Math.abs(a - b) >= 1e-9 }[op];
        return result ? 1 : 0;
      };
    }
    return left;
  }

  function additive(depth) {
    let left = term(depth);
    for (;;) {
      skip();
      if (source[index] === "+" || source[index] === "-") {
        const op = source[index++];
        const l = left;
        const r = term(depth);
        left = op === "+" ? (vars) => l(vars) + r(vars) : (vars) => l(vars) - r(vars);
        continue;
      }
      return left;
    }
  }

  function term(depth) {
    let left = unary(depth);
    for (;;) {
      skip();
      if (source[index] === "*" || source[index] === "/") {
        const op = source[index++];
        const l = left;
        const r = unary(depth);
        left = op === "*" ? (vars) => l(vars) * r(vars) : (vars) => {
          const b = r(vars);
          return b === 0 ? 0 : l(vars) / b;
        };
        continue;
      }
      return left;
    }
  }

  function unary(depth) {
    skip();
    if (source[index] === "-") {
      index++;
      const inner = unary(depth);
      return (vars) => -inner(vars);
    }
    return primary(depth);
  }

  function primary(depth) {
    skip();
    if (index >= source.length) throw new FormulaError("A fórmula termina antes do esperado: falta um valor.", index);
    const current = source[index];

    if (current === "(") {
      index++;
      const inner = expression(depth + 1);
      skip();
      if (source[index] !== ")") throw new FormulaError("Falta fechar o parêntese.", index);
      index++;
      return inner;
    }

    if (/[0-9.]/.test(current)) {
      const start = index;
      while (index < source.length && /[0-9.]/.test(source[index])) index++;
      const text = source.slice(start, index);
      const value = Number(text);
      if (!Number.isFinite(value)) throw new FormulaError(`Número inválido "${text}". Use ponto como separador decimal.`, start);
      return () => value;
    }

    if (/[\p{L}_]/u.test(current)) {
      const start = index;
      while (index < source.length && /[\p{L}\p{N}_]/u.test(source[index])) index++;
      const name = source.slice(start, index).toLowerCase();
      skip();

      if (source[index] !== "(") {
        if (FUNCTIONS[name]) throw new FormulaError(`"${name}" é uma função: use ${name}(...).`, start);
        identifiers.add(name);
        return (vars) => vars[name] ?? 0;
      }

      if (!FUNCTIONS[name]) throw new FormulaError(`Função desconhecida "${name}". Disponíveis: ${Object.keys(FUNCTIONS).join(", ")}.`, start);
      index++;
      const args = [];
      skip();
      if (source[index] === ")") index++;
      else {
        for (;;) {
          args.push(expression(depth + 1));
          skip();
          if (index >= source.length) throw new FormulaError(`Falta fechar o parêntese da função ${name}.`, index);
          if (source[index] === "," || source[index] === ";") {
            index++;
            continue;
          }
          if (source[index] === ")") {
            index++;
            break;
          }
          throw new FormulaError(`Esperava vírgula ou parêntese em ${name}(...), achei "${source[index]}".`, index);
        }
      }

      const [min, max] = FUNCTIONS[name];
      if (args.length < min || args.length > max) {
        throw new FormulaError(`${name}(...) recebe ${min === max ? min : `de ${min} a ${max}`} valor(es), mas recebeu ${args.length}.`, start);
      }

      return (vars) => {
        const values = args.map((argument) => argument(vars));
        switch (name) {
          case "min":
            return Math.min(...values);
          case "max":
            return Math.max(...values);
          case "abs":
            return Math.abs(values[0]);
          case "clamp":
            return Math.min(Math.max(values[0], values[1]), values[2]);
          case "if":
            return values[0] !== 0 ? values[1] : values[2];
          default: {
            const places = values.length === 2 ? Math.min(10, Math.max(0, Math.trunc(values[1]))) : 0;
            const factor = 10 ** places;
            return Math.round(values[0] * factor) / factor;
          }
        }
      };
    }

    throw new FormulaError(`Caractere inesperado "${current}".`, index);
  }

  try {
    const root = expression(0);
    skip();
    if (index < source.length) throw new FormulaError(`Caractere inesperado "${source[index]}".`, index);
    return { evaluate: (vars) => (Number.isFinite(root(vars)) ? root(vars) : 0), identifiers };
  } catch (error) {
    if (error instanceof FormulaError) return { error: { message: error.message, position: error.position } };
    throw error;
  }
}

const POST_DATA = ["postPillar", "postOee"];
const LINE_DATA = ["linePillar", "lineOee"];
const AGGREGATED = ["availability", "performance", "quality", "machine_oee"];
const MODES = ["average", "sum", "product", "last_post"];

const BASE_VARIABLES = [
  ["elapsed_seconds", "Tempo decorrido", "Segundos entre o início da produção e agora (ou o encerramento).", "s", POST_DATA],
  ["oee_time_rate_seconds", "Tempo de referência do turno", "Base de tempo do turno. Hoje é igual ao tempo decorrido.", "s", POST_DATA],
  ["planned_stop_seconds", "Parada planejada", "Segundos de paradas planejadas. Ainda não há pausas cadastradas, então vale 0.", "s", POST_DATA],
  ["occupied_seconds", "Tempo com gente no posto", "Segundos em que alguém estava alocado no posto.", "s", POST_DATA],
  ["downtime_availability", "Parada que afeta a disponibilidade", "Segundos de parada marcados como perda de disponibilidade. Vem do help desk.", "s", POST_DATA],
  ["downtime_performance", "Parada que afeta o desempenho", "Segundos de parada marcados como perda de desempenho (microparadas). Vem do help desk.", "s", POST_DATA],
  ["total_downtime", "Parada total", "Soma de todas as paradas do posto. Vem do help desk.", "s", POST_DATA],
  ["real_downtime", "Parada real", "Segundos de parada medidos pela máquina. Vem do help desk.", "s", POST_DATA],
  ["ticket_downtime_proportional", "Parada de chamados", "Segundos de parada abertos por chamados. Vem do help desk.", "s", POST_DATA],
  ["ticket_downtime_pieces", "Peças perdidas por chamados", "Meta por hora × parada de chamados, em peças.", "pç", POST_DATA],
  ["real_production", "Produzido", "Soma do que foi lançado no posto no período.", "pç", POST_DATA],
  ["theoretical_production", "Produção teórica", "Meta por hora × horas decorridas.", "pç", POST_DATA],
  ["expected_production", "Produção esperada", "Meta por hora × horas com gente no posto.", "pç", POST_DATA],
  ["expected_production_min", "Meta mínima", "Produção esperada × 0,9.", "pç", POST_DATA],
  ["max_rate_hour", "Capacidade máxima", "Meta por hora × horas decorridas (igual à teórica por enquanto).", "pç", POST_DATA],
  ["defects", "Defeitos", "Peças com defeito de qualidade no período. Vem do help desk.", "pç", POST_DATA],
  ["repair_defects", "Defeitos de retrabalho", "Peças com defeito encontrado no reparo. Vem do help desk.", "pç", POST_DATA],
  ["availability", "Disponibilidade", "Pilar de disponibilidade já calculado (razão de 0 a 1).", "razão", ["postOee", "lineOee"]],
  ["performance", "Desempenho", "Pilar de desempenho já calculado (razão de 0 a 1).", "razão", ["postOee", "lineOee"]],
  ["quality", "Qualidade", "Pilar de qualidade já calculado (razão de 0 a 1).", "razão", ["postOee", "lineOee"]],
  ["machine_oee", "OEE médio dos postos", "Média do OEE dos postos medidos (razão de 0 a 1).", "razão", ["lineOee"]],
  ["line_plan", "Plano da linha", "Produção teórica do último posto da linha.", "pç", LINE_DATA],
  ["line_produced", "Produzido na linha", "Soma do produzido em todos os postos medidos.", "pç", LINE_DATA],
  ["line_produced_station", "Produzido no último posto", "Produzido no último posto medido do fluxo.", "pç", LINE_DATA],
  ["line_elapsed", "Tempo decorrido da linha", "Segundos da produção.", "s", LINE_DATA],
  ["line_downtime_availability", "Paradas de disponibilidade da linha", "Soma nos postos medidos.", "s", LINE_DATA],
  ["line_downtime_performance", "Paradas de desempenho da linha", "Soma nos postos medidos.", "s", LINE_DATA],
  ["line_total_downtime", "Parada total da linha", "Soma nos postos medidos.", "s", LINE_DATA],
  ["line_defects", "Defeitos da linha", "Soma nos postos medidos.", "pç", LINE_DATA],
];

const PILLAR_LABEL = { availability: "disponibilidade", performance: "desempenho", quality: "qualidade", machine_oee: "OEE do posto" };
const MODE_LABEL = { average: "média", sum: "soma", product: "produto", last_post: "último posto" };

const VARIABLES = [
  ...BASE_VARIABLES,
  ...AGGREGATED.flatMap((pillar) =>
    MODES.map((mode) => [
      `${pillar}_${mode}`,
      `${PILLAR_LABEL[pillar][0].toUpperCase()}${PILLAR_LABEL[pillar].slice(1)} (${MODE_LABEL[mode]})`,
      `Agrega a ${PILLAR_LABEL[pillar]} dos postos medidos pela ${MODE_LABEL[mode]}. Razão de 0 a 1.`,
      "razão",
      LINE_DATA,
    ]),
  ),
];

const SLOTS = [
  { key: "availability", label: "disponibilidade do posto", scope: "postPillar", group: "post" },
  { key: "performance", label: "desempenho do posto", scope: "postPillar", group: "post" },
  { key: "quality", label: "qualidade do posto", scope: "postPillar", group: "post" },
  { key: "machineOee", label: "OEE do posto", scope: "postOee", group: "post" },
  { key: "lineAvailability", label: "disponibilidade da linha", scope: "linePillar", group: "line" },
  { key: "linePerformance", label: "desempenho da linha", scope: "linePillar", group: "line" },
  { key: "lineQuality", label: "qualidade da linha", scope: "linePillar", group: "line" },
  { key: "lineOee", label: "OEE da linha", scope: "lineOee", group: "line" },
];

const DEFAULT_FORMULAS = {
  availability: "(elapsed_seconds - downtime_availability) / elapsed_seconds",
  performance: "real_production / theoretical_production",
  quality: "if(real_production > 0, (real_production - defects) / real_production, 1)",
  machineOee: "availability * performance * quality",
  lineAvailability: "availability_average",
  linePerformance: "performance_last_post",
  lineQuality: "quality_product",
  lineOee: "availability * performance * quality",
};

const allowedIn = (scope) => new Set(VARIABLES.filter((variable) => variable[4].includes(scope)).map((variable) => variable[0]));

function validateFormula(source, scope) {
  const parsed = parse(source);
  if (parsed.error) return parsed.error;
  const allowed = allowedIn(scope);
  for (const identifier of parsed.identifiers) {
    if (!allowed.has(identifier)) return { message: `"${identifier}" não existe neste tipo de fórmula.`, position: Math.max(0, source.indexOf(identifier)) };
  }
  return null;
}

function checkFormulas(formulas) {
  for (const slot of SLOTS) {
    const error = validateFormula(formulas?.[slot.key], slot.scope);
    if (error) return `Fórmula de ${slot.label}: ${error.message}`;
  }
  return null;
}

function compile(source, fallback) {
  return parse(source).evaluate ? parse(source) : parse(fallback);
}

function calculator(formulas) {
  const c = Object.fromEntries(SLOTS.map((slot) => [slot.key, compile(formulas[slot.key], DEFAULT_FORMULAS[slot.key])]));
  const pillars = (vars) => {
    const availability = Math.max(0, c.availability.evaluate(vars));
    const performance = Math.max(0, c.performance.evaluate(vars));
    const quality = Math.max(0, c.quality.evaluate(vars));
    const oee = Math.max(0, c.machineOee.evaluate({ ...vars, availability, performance, quality }));
    return { availability, performance, quality, oee };
  };

  const linePillars = (vars) => {
    const availability = Math.max(0, c.lineAvailability.evaluate(vars));
    const performance = Math.max(0, c.linePerformance.evaluate(vars));
    const quality = Math.max(0, c.lineQuality.evaluate(vars));
    const oee = Math.max(0, c.lineOee.evaluate({ ...vars, availability, performance, quality, machine_oee: vars.machine_oee_average ?? 0 }));
    return { availability, performance, quality, oee };
  };

  const overlap = (a, b, from, to) => Math.max(0, (Math.min(b, to) - Math.max(a, from)) / 1000);

  const postVars = (run, post, from, to, slot, losses, lastPostId) => {
    const elapsed = Math.max((to - from) / 1000, 0);
    const target = post.targetPerHour ?? 0;
    const occupied = run.allocations.filter((item) => item.postId === post.id).reduce((sum, item) => sum + overlap(item.allocatedAt, item.releasedAt ?? to, from, to), 0);
    const produced = run.outputs.filter((item) => item.postId === post.id && (slot === undefined || item.hourStart === slot)).reduce((sum, item) => sum + item.quantity, 0);
    const mine = (losses?.downtimes ?? []).filter((item) => item.postId == null || item.postId === post.id);
    const downtime = mine.reduce((sum, item) => sum + overlap(item.from, item.to, from, to), 0);
    const defectCount = (losses?.defects ?? []).filter((item) => (item.postId === post.id || (item.postId == null && post.id === lastPostId)) && item.at >= from && (slot === undefined ? item.at <= to : item.at < to)).reduce((sum, item) => sum + item.quantity, 0);
    const theoretical = (target * elapsed) / 3600;
    const expected = (target * occupied) / 3600;
    return {
      elapsed_seconds: elapsed,
      oee_time_rate_seconds: elapsed,
      planned_stop_seconds: 0,
      occupied_seconds: occupied,
      downtime_availability: downtime,
      downtime_performance: 0,
      total_downtime: downtime,
      real_downtime: 0,
      ticket_downtime_proportional: downtime,
      ticket_downtime_pieces: (target / 3600) * downtime,
      real_production: produced,
      theoretical_production: theoretical,
      expected_production: expected,
      expected_production_min: expected * 0.9,
      max_rate_hour: theoretical,
      defects: defectCount,
      repair_defects: 0,
    };
  };

  const lineVars = (measured, lastPillars, valuesList, elapsed, produced, plan, stationProduced) => {
    const vars = {
      line_plan: plan,
      line_produced: produced,
      line_produced_station: stationProduced,
      line_elapsed: elapsed,
      line_downtime_availability: measured.reduce((sum, item) => sum + (item.vars.downtime_availability ?? 0), 0),
      line_downtime_performance: 0,
      line_total_downtime: measured.reduce((sum, item) => sum + (item.vars.total_downtime ?? 0), 0),
      line_defects: measured.reduce((sum, item) => sum + (item.vars.defects ?? 0), 0),
    };
    for (const [pillar, pick] of [["availability", (v) => v.availability], ["performance", (v) => v.performance], ["quality", (v) => v.quality], ["machine_oee", (v) => v.oee]]) {
      const ratios = valuesList.map(pick);
      vars[`${pillar}_average`] = ratios.length ? ratios.reduce((s, v) => s + v, 0) / ratios.length : 0;
      vars[`${pillar}_sum`] = ratios.reduce((s, v) => s + v, 0);
      vars[`${pillar}_product`] = ratios.length ? ratios.reduce((s, v) => s * v, 1) : 0;
      vars[`${pillar}_last_post`] = lastPillars ? pick(lastPillars) : vars[`${pillar}_average`];
    }
    return vars;
  };

  const calculate = (run, now, losses) => {
    if (!run.startedAt) {
      return { started: false, from: null, to: null, elapsedSeconds: 0, posts: run.snapshot.posts.map((post) => ({ post, measured: false, pillars: { availability: 0, performance: 0, quality: 0, oee: 0 }, vars: {}, hours: [] })), line: { pillars: { availability: 0, performance: 0, quality: 0, oee: 0 }, vars: {}, hours: [], lastPostId: null } };
    }

    const from = new Date(run.startedAt).getTime();
    const to = run.endedAt ? new Date(run.endedAt).getTime() : now;
    const slots = [];
    for (let slot = truncateHour(from); slot < to || slots.length === 0; slot += HOUR_MS) {
      slots.push(slot);
      if (slots.length >= 96) break;
    }

    const lastPostId = run.snapshot.posts.filter((post) => (post.targetPerHour ?? 0) > 0).sort((a, b) => a.x - b.x || a.id - b.id).at(-1)?.id;
    const posts = run.snapshot.posts.map((post) => {
      const measured = (post.targetPerHour ?? 0) > 0;
      const vars = postVars(run, post, from, to, undefined, losses, lastPostId);
      const hours = measured
        ? slots.map((slot) => {
            const wFrom = Math.max(slot, from);
            const wTo = Math.min(slot + HOUR_MS, to);
            const slotVars = postVars(run, post, wFrom, wTo, slot, losses, lastPostId);
            return { slot, seconds: Math.max((wTo - wFrom) / 1000, 0), pillars: pillars(slotVars), produced: slotVars.real_production, theoretical: slotVars.theoretical_production };
          })
        : [];
      return { post, measured, pillars: measured ? pillars(vars) : { availability: 0, performance: 0, quality: 0, oee: 0 }, vars, hours };
    });

    const measured = posts.filter((item) => item.measured).sort((a, b) => a.post.x - b.post.x || a.post.id - b.post.id);
    const last = measured.at(-1);
    const vars = lineVars(measured, last?.pillars, measured.map((item) => item.pillars), (to - from) / 1000, measured.reduce((s, item) => s + item.vars.real_production, 0), last?.vars.theoretical_production ?? 0, last?.vars.real_production ?? 0);

    const hours = slots.map((slot) => {
      const perPost = measured.map((item) => item.hours.find((hour) => hour.slot === slot)).filter(Boolean);
      const lastHour = last?.hours.find((hour) => hour.slot === slot);
      const seconds = perPost.length ? Math.max(...perPost.map((hour) => hour.seconds)) : 0;
      const hourVars = lineVars(measured, lastHour?.pillars, perPost.map((hour) => hour.pillars), seconds, perPost.reduce((s, hour) => s + hour.produced, 0), lastHour?.theoretical ?? 0, lastHour?.produced ?? 0);
      return { slot, seconds, pillars: linePillars(hourVars), produced: perPost.reduce((s, hour) => s + hour.produced, 0), theoretical: lastHour?.theoretical ?? 0 };
    });

    return { started: true, from, to, elapsedSeconds: Math.max((to - from) / 1000, 0), posts, line: { pillars: linePillars(vars), vars, hours, lastPostId: last?.post.id ?? null } };
  };

  return { calculate };
}

const percent = (ratio) => Math.round(ratio * 10_000) / 100;
const pillarsOut = (p) => ({ availability: percent(p.availability), performance: percent(p.performance), quality: percent(p.quality), oee: percent(p.oee) });
const hourOut = (hour) => ({ hourStart: iso(hour.slot), seconds: Math.round(hour.seconds), pillars: pillarsOut(hour.pillars), produced: Math.round(hour.produced * 100) / 100, theoretical: Math.round(hour.theoretical * 100) / 100 });

export function createOeeMock(users, productionData, helpdeskData) {
  let counter = 7000;
  const next = () => ++counter;
  const nameOf = (id) => users.find((user) => user.id === id)?.name ?? null;
  const sets = [];

  const seedHistory = () => {
    const demo = productionData.runs.find((run) => run.status === "running");
    if (!demo) return;
    const factors = [
      { lineId: 1, productId: 1, days: 1, scale: 0.92, hours: 7 },
      { lineId: 2, productId: 2, days: 2, scale: 0.74, hours: 6 },
      { lineId: 1, productId: 1, days: 3, scale: 0.55, hours: 8 },
    ];
    for (const [index, item] of factors.entries()) {
      const start = truncateHour(Date.now() - item.days * 86_400_000 - 6 * HOUR_MS);
      const run = {
        id: next(),
        layoutId: demo.layoutId,
        lineId: item.lineId,
        productId: item.productId,
        shiftId: 1,
        status: "closed",
        snapshot: structuredClone(demo.snapshot),
        createdAt: iso(start - HOUR_MS),
        createdBy: 1,
        startedAt: iso(start),
        startedBy: 1,
        endedAt: iso(start + item.hours * HOUR_MS),
        endedBy: 1,
        allocations: [],
        outputs: [],
      };
      for (const post of run.snapshot.posts) {
        const userId = 11 + (post.id % 5);
        run.allocations.push({ id: next(), postId: post.id, userId, allocatedAt: start, releasedAt: start + item.hours * HOUR_MS, allocatedBy: 1 });
        for (let hour = 0; hour < item.hours; hour++) {
          const wobble = 0.85 + ((hour * 7 + index * 3 + post.id) % 5) * 0.06;
          run.outputs.push({ id: next(), postId: post.id, hourStart: start + hour * HOUR_MS, quantity: Math.round((post.targetPerHour ?? 0) * item.scale * wobble), userId, recordedBy: 1, recordedAt: iso(start + (hour + 1) * HOUR_MS) });
        }
      }
      productionData.runs.push(run);
    }
  };

  seedHistory();

  const active = () => sets.find((set) => set.isActive);
  const currentCalculator = () => calculator(active()?.formulas ?? DEFAULT_FORMULAS);
  const setName = () => active()?.name ?? "Padrão";

  const lossesOf = (run, now) => (run.startedAt ? helpdeskData.lossesFor(run.lineId, new Date(run.startedAt).getTime(), run.endedAt ? new Date(run.endedAt).getTime() : now) : { downtimes: [], defects: [] });

  const lookup = (run) => ({
    line: productionData.lines.find((item) => item.id === run.lineId),
    product: productionData.products.find((item) => item.id === run.productId),
    shift: productionData.shifts.find((item) => item.id === run.shiftId),
  });

  const runDetail = (run, now) => {
    const { line, product, shift } = lookup(run);
    const oee = currentCalculator().calculate(run, now, lossesOf(run, now));
    return {
      runId: run.id,
      status: run.status,
      lineId: run.lineId,
      lineName: line?.name ?? "",
      productCode: product?.code ?? "",
      productName: product?.name ?? "",
      shiftName: shift?.name ?? "",
      startedAt: run.startedAt,
      endedAt: run.endedAt,
      elapsedSeconds: Math.round(oee.elapsedSeconds),
      line: pillarsOut(oee.line.pillars),
      produced: Math.round((oee.line.vars.line_produced_station ?? 0) * 100) / 100,
      theoretical: Math.round((oee.line.vars.line_plan ?? 0) * 100) / 100,
      lastPostId: oee.line.lastPostId,
      posts: oee.posts.map((item) => ({
        postId: item.post.id,
        name: item.post.name,
        kind: item.post.kind,
        x: item.post.x,
        targetPerHour: item.post.targetPerHour,
        measured: item.measured,
        pillars: pillarsOut(item.pillars),
        produced: Math.round((item.vars.real_production ?? 0) * 100) / 100,
        theoretical: Math.round((item.vars.theoretical_production ?? 0) * 100) / 100,
        occupiedSeconds: Math.round(item.vars.occupied_seconds ?? 0),
        downtimeSeconds: 0,
        defects: 0,
        hours: item.hours.map(hourOut),
      })),
      hours: oee.line.hours.map(hourOut),
      lossSourceAvailable: true,
      formulaSetName: setName(),
      _oee: oee,
    };
  };

  const summary = (run, now) => {
    const detail = runDetail(run, now);
    return {
      runId: detail.runId,
      status: detail.status,
      lineId: detail.lineId,
      lineName: detail.lineName,
      productCode: detail.productCode,
      productName: detail.productName,
      shiftName: detail.shiftName,
      startedAt: detail.startedAt,
      endedAt: detail.endedAt,
      elapsedSeconds: detail.elapsedSeconds,
      pillars: detail.line,
      produced: detail.produced,
      theoretical: detail.theoretical,
      measuredPosts: detail.posts.filter((post) => post.measured).length,
      totalPosts: detail.posts.length,
    };
  };

  const formulasOut = (formulas) => ({ ...formulas });
  const setOut = (set) => ({ id: set.id, name: set.name, isActive: set.isActive, formulas: formulasOut(set.formulas), updatedAt: set.updatedAt, updatedByName: nameOf(set.updatedBy) });

  const cleanFormulas = (formulas) => Object.fromEntries(SLOTS.map((slot) => [slot.key, String(formulas?.[slot.key] ?? "").trim()]));

  const readSet = (body, exceptId) => {
    const name = String(body?.name ?? "").trim();
    if (!name) return { problem: "Informe o nome do conjunto de fórmulas." };
    if (name.length > MAX_SET_NAME) return { problem: `O nome pode ter no máximo ${MAX_SET_NAME} caracteres.` };
    if (!body.formulas) return { problem: "Escreva a fórmula." };
    const formulas = cleanFormulas(body.formulas);
    const problem = checkFormulas(formulas);
    if (problem) return { problem };
    if (sets.some((set) => set.name.toLowerCase() === name.toLowerCase() && set.id !== exceptId)) return { problem: `Já existe um conjunto de fórmulas chamado "${name}".` };
    return { name, formulas };
  };

  return function handle({ req, res, url, segments, actor, json, send, fail }) {
    const [, , , a, b, c] = segments;
    const canManage = actor !== 3;
    const refuse = (message, status = 400, title = "Falha na operação") => fail(res, status, title, message);
    const denied = () => fail(res, 403, "Acesso negado", "Sua matrícula não tem permissão para esta operação.");
    const now = Date.now();

    if (a === "config" && req.method === "GET") {
      return send(res, 200, {
        sets: sets.map(setOut),
        activeSetId: active()?.id ?? null,
        default: formulasOut(DEFAULT_FORMULAS),
        variables: VARIABLES.map(([key, label, description, unit, scopes]) => ({
          key,
          label,
          description,
          unit,
          slots: SLOTS.filter((slot) => scopes.includes(slot.scope)).map((slot) => slot.key),
        })),
        slots: SLOTS.map(({ key, label, group }) => ({ key, label, group })),
        functions: FUNCTION_DOCS,
        lossSourceAvailable: true,
        canManage,
      });
    }

    if (a === "runs" && req.method === "GET" && !b) {
      const status = url.searchParams.get("status");
      const lineId = Number(url.searchParams.get("lineId") ?? 0);
      const to = url.searchParams.get("to") ? new Date(`${url.searchParams.get("to")}T23:59:59.999Z`).getTime() : now;
      const from = url.searchParams.get("from") ? new Date(`${url.searchParams.get("from")}T00:00:00.000Z`).getTime() : to - 7 * 86_400_000;
      if (Number.isNaN(from) || Number.isNaN(to) || to < from) return refuse("O período precisa ter início e fim, e o fim não pode ser anterior ao início.");
      if ((to - from) / 86_400_000 > 92) return refuse("O período pode ter no máximo 92 dias.");

      const runs = productionData.runs
        .filter((run) => run.startedAt && (!status || run.status === status) && (!lineId || run.lineId === lineId))
        .filter((run) => new Date(run.startedAt).getTime() <= to && (!run.endedAt || new Date(run.endedAt).getTime() >= from))
        .sort((x, y) => new Date(y.startedAt) - new Date(x.startedAt))
        .slice(0, 60)
        .map((run) => summary(run, now));

      return send(res, 200, { from: iso(from), to: iso(to), runs, lossSourceAvailable: true, formulaSetName: setName() });
    }

    if (a === "runs" && req.method === "GET" && b) {
      const run = productionData.runs.find((item) => String(item.id) === b);
      if (!run) return refuse("Produção não encontrada.", 404, "Falha ao calcular o OEE");
      const detail = runDetail(run, now);
      delete detail._oee;
      return send(res, 200, detail);
    }

    if (a === "formulas") {
      if (b === "test" && req.method === "POST") {
        const slot = SLOTS.find((item) => item.key === json?.slot);
        if (!slot) return refuse("Escopo de fórmula inválido.");
        const parsed = parse(json?.expression);
        const empty = { valid: false, error: null, position: null, identifiers: [], result: null, values: {} };
        if (parsed.error) return send(res, 200, { ...empty, error: parsed.error.message, position: parsed.error.position });
        const scopeError = validateFormula(json.expression, slot.scope);
        if (scopeError) return send(res, 200, { ...empty, error: scopeError.message, position: scopeError.position, identifiers: [...parsed.identifiers] });

        const values = {};
        if (json.runId) {
          const run = productionData.runs.find((item) => item.id === json.runId);
          if (!run) return refuse("Produção não encontrada.", 404);
          if (!run.startedAt) return refuse("A produção ainda não foi iniciada, então não há OEE para calcular.");
          const oee = currentCalculator().calculate(run, now, lossesOf(run, now));
          if (slot.scope.startsWith("post")) {
            const item = oee.posts.find((candidate) => candidate.post.id === json.postId) ?? oee.posts.filter((candidate) => candidate.measured).sort((x, y) => x.post.x - y.post.x)[0];
            if (item) Object.assign(values, item.vars, { availability: item.pillars.availability, performance: item.pillars.performance, quality: item.pillars.quality });
          } else {
            Object.assign(values, oee.line.vars, oee.line.pillars, { machine_oee: oee.line.vars.machine_oee_average ?? 0 });
          }
        }

        const used = [...parsed.identifiers].sort();
        const evaluated = Object.fromEntries(used.map((key) => [key, json.runId ? (values[key] ?? 0) : (json.values?.[key] ?? 0)]));
        return send(res, 200, { valid: true, error: null, position: null, identifiers: used, result: Math.round(parsed.evaluate(evaluated) * 1e6) / 1e6, values: evaluated });
      }

      if (!canManage) return denied();

      if (b === "active" && req.method === "PUT") {
        const id = json?.setId ?? null;
        if (id !== null && !sets.some((set) => set.id === id)) return refuse("Conjunto de fórmulas não encontrado.", 404);
        for (const set of sets) set.isActive = set.id === id;
        return send(res, 204);
      }

      if (req.method === "POST" && !b) {
        const read = readSet(json);
        if (read.problem) return refuse(read.problem, 400, "Falha ao criar o conjunto de fórmulas");
        if (json.activate) for (const set of sets) set.isActive = false;
        const created = { id: next(), name: read.name, isActive: Boolean(json.activate), formulas: read.formulas, updatedAt: iso(now), updatedBy: actor };
        sets.push(created);
        return send(res, 201, setOut(created));
      }

      const set = sets.find((item) => String(item.id) === b);
      if (!set) return refuse("Conjunto de fórmulas não encontrado.", 404);

      if (req.method === "PUT" && !c) {
        const read = readSet(json, set.id);
        if (read.problem) return refuse(read.problem, 400, "Falha ao salvar o conjunto de fórmulas");
        Object.assign(set, { name: read.name, formulas: read.formulas, updatedAt: iso(now), updatedBy: actor });
        if (json.activate && !set.isActive) {
          for (const other of sets) other.isActive = false;
          set.isActive = true;
        }
        return send(res, 204);
      }

      if (req.method === "DELETE" && !c) {
        if (set.isActive) return refuse("O conjunto ativo não pode ser excluído. Ative outro ou volte ao padrão antes.");
        sets.splice(sets.indexOf(set), 1);
        return send(res, 204);
      }
    }

    return fail(res, 404, "Rota inexistente", url.pathname);
  };
}
