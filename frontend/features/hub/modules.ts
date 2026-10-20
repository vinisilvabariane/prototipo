export type ModuleStatus = "active" | "soon";

export type ModuleFunction = { label: string; href?: string };

export type HubModule = {
  code: string;
  name: string;
  description: string;
  status: ModuleStatus;
  functions: ModuleFunction[];
};

export type HubGroup = { id: string; name: string; description: string; modules: HubModule[] };

export const hubGroups: HubGroup[] = [
  {
    id: "administracao",
    name: "Administração",
    description: "Acesso e configurações",
    modules: [
      {
        code: "ACE",
        name: "Controle de acesso",
        description: "Quem entra no sistema e o que cada pessoa pode fazer.",
        status: "active",
        functions: [
          { label: "Usuários", href: "/users" },
          { label: "Perfis", href: "/roles" },
          { label: "Departamentos", href: "/departments" },
        ],
      },
      {
        code: "ORG",
        name: "Organograma",
        description: "Cargos, quem ocupa cada um e quem responde a quem.",
        status: "active",
        functions: [{ label: "Organograma", href: "/org-chart" }],
      },
      {
        code: "CFG",
        name: "Configurações",
        description: "Aparência e acessibilidade do sistema. Em breve, plantas, áreas e turnos da fábrica.",
        status: "active",
        functions: [
          { label: "Aparência e acessibilidade", href: "/settings" },
          { label: "Plantas e áreas" },
          { label: "Turnos" },
          { label: "Parâmetros do sistema" },
        ],
      },
    ],
  },
  {
    id: "planejamento",
    name: "Planejamento",
    description: "Ordens, materiais e roteiros",
    modules: [
      {
        code: "PCP",
        name: "Planejamento da produção",
        description: "Ordens, sequenciamento e capacidade por linha.",
        status: "soon",
        functions: [{ label: "Ordens de produção" }, { label: "Sequenciamento" }, { label: "Capacidade" }],
      },
      {
        code: "MRP",
        name: "Necessidade de materiais",
        description: "O que comprar e quando, a partir das ordens.",
        status: "soon",
        functions: [{ label: "Explosão de materiais" }, { label: "Cobertura de estoque" }, { label: "Solicitações de compra" }],
      },
      {
        code: "ROT",
        name: "Roteiros e receitas",
        description: "Como cada produto é fabricado.",
        status: "soon",
        functions: [{ label: "Roteiros de fabricação" }, { label: "Estrutura de produto" }, { label: "Parâmetros de processo" }],
      },
    ],
  },
  {
    id: "engenharia",
    name: "Engenharia",
    description: "Processos e aprovações",
    modules: [
      {
        code: "WFL",
        name: "Workflow",
        description: "Fluxos de aprovação com responsáveis, justificativas e histórico completo.",
        status: "active",
        functions: [{ label: "Workflows", href: "/workflows" }],
      },
      {
        code: "INT",
        name: "Instruções de trabalho",
        description: "Como cada operação é executada: EPI, especificações, passos ilustrados e PDF.",
        status: "active",
        functions: [{ label: "Instruções de trabalho", href: "/instructions" }],
      },
    ],
  },
  {
    id: "producao",
    name: "Produção",
    description: "Postos do chão de fábrica",
    modules: [
      {
        code: "APT",
        name: "Apontamento",
        description: "Registro de produção, paradas e refugo por posto.",
        status: "soon",
        functions: [{ label: "Apontar produção" }, { label: "Paradas e refugos" }, { label: "Troca de turno" }],
      },
      {
        code: "LIN",
        name: "Linhas e postos",
        description: "Monte o mapa de cada linha, aloque pessoas nos postos, inicie a produção e acompanhe a produtividade por posto.",
        status: "active",
        functions: [
          { label: "Painel de produção", href: "/production" },
          { label: "Layouts de linha", href: "/production/layouts" },
          { label: "Cadastros de produção", href: "/production/setup" },
        ],
      },
      {
        code: "HLP",
        name: "Help desk",
        description: "Chamados para manutenção, qualidade e outros setores, com formulário montado por setor e parada de linha que alimenta o OEE.",
        status: "active",
        functions: [
          { label: "Abrir chamado", href: "/helpdesk" },
          { label: "Quadro de atendimento", href: "/helpdesk/board" },
          { label: "Meus chamados", href: "/helpdesk/tickets" },
          { label: "Setores e formulários", href: "/helpdesk/sectors" },
        ],
      },
      {
        code: "OEE",
        name: "Eficiência (OEE)",
        description: "Disponibilidade, desempenho e qualidade por linha e por posto, com fórmulas que você mesmo monta.",
        status: "active",
        functions: [
          { label: "Comparativo entre linhas", href: "/oee" },
          { label: "OEE em tempo real", href: "/oee/live" },
          { label: "Fórmulas de OEE", href: "/oee/formulas" },
        ],
      },
    ],
  },
  {
    id: "qualidade",
    name: "Qualidade",
    description: "Fornecedores, inspeção e metrologia",
    modules: [
      {
        code: "FOR",
        name: "Fornecedores",
        description: "Cadastro, auto avaliações, auditorias, certificados e documentos de cada fornecedor.",
        status: "active",
        functions: [{ label: "Fornecedores", href: "/suppliers" }],
      },
      {
        code: "INS",
        name: "Inspeção",
        description: "Planos e registros de inspeção ao longo do processo.",
        status: "soon",
        functions: [{ label: "Planos de inspeção" }, { label: "Inspeção em processo" }, { label: "Inspeção final" }],
      },
      {
        code: "NCO",
        name: "Não conformidades",
        description: "Do registro à ação corretiva.",
        status: "soon",
        functions: [{ label: "Registro" }, { label: "Contenção" }, { label: "Ações corretivas" }],
      },
      {
        code: "MET",
        name: "Metrologia",
        description: "Instrumentos de medição e suas calibrações.",
        status: "soon",
        functions: [{ label: "Instrumentos" }, { label: "Calendário de calibração" }, { label: "Certificados" }],
      },
    ],
  },
  {
    id: "manutencao",
    name: "Manutenção",
    description: "Ordens e equipamentos",
    modules: [
      {
        code: "MNT",
        name: "Ordens de manutenção",
        description: "Corretivas, preventivas e preditivas.",
        status: "soon",
        functions: [{ label: "Abrir ordem" }, { label: "Plano preventivo" }, { label: "Histórico por ativo" }],
      },
      {
        code: "ATV",
        name: "Ativos",
        description: "Cadastro e vida útil dos equipamentos.",
        status: "soon",
        functions: [{ label: "Equipamentos" }, { label: "Peças de reposição" }, { label: "Contadores de uso" }],
      },
    ],
  },
  {
    id: "logistica",
    name: "Materiais e logística",
    description: "Estoque e expedição",
    modules: [
      {
        code: "ALM",
        name: "Almoxarifado",
        description: "Estoque de matéria-prima e componentes.",
        status: "soon",
        functions: [{ label: "Entradas" }, { label: "Saídas" }, { label: "Inventário" }],
      },
      {
        code: "ABS",
        name: "Abastecimento de linha",
        description: "Reposição de material nos postos.",
        status: "soon",
        functions: [{ label: "Kanban" }, { label: "Requisições" }, { label: "Supermercado" }],
      },
      {
        code: "EXP",
        name: "Expedição",
        description: "Separação, embarque e documentos.",
        status: "soon",
        functions: [{ label: "Romaneios" }, { label: "Embarques" }, { label: "Etiquetas" }],
      },
    ],
  },
  {
    id: "rastreabilidade",
    name: "Rastreabilidade",
    description: "Lotes, séries e documentos",
    modules: [
      {
        code: "TRC",
        name: "Rastreabilidade",
        description: "De qual lote veio cada peça e para onde ela foi.",
        status: "soon",
        functions: [{ label: "Genealogia de lotes" }, { label: "Número de série" }, { label: "Bloqueio e recall" }],
      },
      {
        code: "DOC",
        name: "Documentos técnicos",
        description: "Instruções de trabalho e desenhos vigentes.",
        status: "soon",
        functions: [{ label: "Instruções de trabalho" }, { label: "Desenhos" }, { label: "Controle de revisões" }],
      },
    ],
  },
  {
    id: "indicadores",
    name: "Indicadores",
    description: "Painéis e relatórios",
    modules: [
      {
        code: "IND",
        name: "Indicadores e relatórios",
        description: "Painéis de gestão e exportações.",
        status: "soon",
        functions: [{ label: "Painéis" }, { label: "Relatórios" }, { label: "Exportações" }],
      },
    ],
  },
];

export const allModules = hubGroups.flatMap((group) => group.modules);
export const availableModules = allModules.filter((module) => module.status === "active");

export type HubEntry = { group: HubGroup; module: HubModule; fn: ModuleFunction };

export const allEntries: HubEntry[] = hubGroups.flatMap((group) =>
  group.modules.flatMap((module) => module.functions.map((fn) => ({ group, module, fn }))),
);

export const isLinkable = (entry: HubEntry) => entry.module.status === "active" && Boolean(entry.fn.href);

export const findEntry = (href: string) => allEntries.find((entry) => entry.fn.href === href);
