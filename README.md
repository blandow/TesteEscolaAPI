# TesteEscolaAPI

API REST de controle de matrículas escolares.

---

## Índice

- [Visão geral](#visão-geral)
- [Stack e principais pacotes](#stack-e-principais-pacotes)
- [Arquitetura e padrões de projeto](#arquitetura-e-padrões-de-projeto)
- [Estrutura de pastas](#estrutura-de-pastas)
- [Pré-requisitos](#pré-requisitos)
- [Como rodar o projeto](#como-rodar-o-projeto)
  - [1. Criar o banco de dados](#1-criar-o-banco-de-dados)
  - [2. Configurar a connection string](#2-configurar-a-connection-string)
  - [3. Restaurar pacotes e rodar](#3-restaurar-pacotes-e-rodar)
- [Endpoints](#endpoints)
  - [Alunos](#alunos)
  - [Turmas](#turmas)
  - [Matrículas](#matrículas)
  - [Relatórios](#relatórios)
- [Regras de negócio da matrícula](#regras-de-negócio-da-matrícula)
- [Testando com o Postman](#testando-com-o-postman)
- [Testes unitários](#testes-unitários)
- [Decisões técnicas e justificativas](#decisões-técnicas-e-justificativas)
- [O que não foi implementado e por quê](#o-que-não-foi-implementado-e-por-quê)
- [Possíveis evoluções](#possíveis-evoluções)

---

## Visão geral

O sistema gerencia o cadastro de alunos, a listagem de turmas com controle de vagas, o processo de matrícula (com validação de regras de negócio dentro de uma transação) e um relatório consolidado de alunos por turma.

Principais funcionalidades entregues:

- CRUD completo de Alunos, com paginação, filtro por nome e exclusão lógica
- Listagem de Turmas com vagas restantes
- Matrícula de aluno em turma, com três regras de negócio validadas dentro de uma transação atômica
- Relatório de alunos matriculados por turma, calculado inteiramente via SQL (`JOIN` + `GROUP BY`)
- Testes unitários (xUnit/MSTest + Moq) cobrindo os principais cenários da regra de matrícula

## Stack e principais pacotes

| Tecnologia | Versão |
|---|---|
| .NET Framework | 4.8 |
| ASP.NET Web API | 5.2.9 |
| Dapper | 2.1.79 |
| SQL Server | LocalDB / Express (qualquer edição) |
| Moq | 4.20.72 |
| MSTest (Framework + TestAdapter) | 2.2.10 |

Nenhum ORM foi utilizado no acesso a dados — todas as queries em `Repositories/` são SQL puro executado via Dapper.

## Arquitetura e padrões de projeto

O projeto segue uma arquitetura em camadas, separando claramente responsabilidades:

```
Controller  →  Service  →  Repository  →  Banco de Dados (Dapper + SQL puro)
```

- **Controllers**: recebem a requisição HTTP, delegam para o Service e traduzem o resultado (ou a exceção lançada) para o status HTTP correto. Não contêm regra de negócio.
- **Services**: concentram toda a regra de negócio (validações, orquestração de transação, mapeamento entidade → DTO).
- **Repositories**: única camada que conversa com o banco. Cada método executa uma operação SQL específica via Dapper.
- **DTOs** (`Requests` / `Responses`): contrato de entrada e saída da API, desacoplado das entidades internas (`Models`). Isso evita que o schema do banco vaze diretamente para o cliente da API e permite evoluir um lado sem quebrar o outro.
- **Models**: entidades que espelham 1:1 as tabelas do banco, usadas apenas entre Repository e Service.
- **Infrastructure**: `DbConnectionFactory`, responsável por criar conexões (`IDbConnection`) a partir da connection string configurada no `Web.config`.

**Padrões de projeto aplicados:**

- **Repository Pattern** — abstrai o acesso a dados atrás de interfaces (`IAlunoRepository`, `ITurmaRepository`, `IMatriculaRepository`, `IRelatorioRepository`), permitindo trocar a implementação (ou mocká-la em testes) sem tocar no Service.
- **DTO Pattern** — separa o contrato público da API do modelo interno de dados.
- **Factory Pattern** — `DbConnectionFactory` centraliza a criação de conexões, isolando o `Web.config`/`ConfigurationManager` do resto da aplicação.
- **Dependency Injection via construtor** — todas as classes recebem suas dependências (`interfaces`) pelo construtor. Não foi utilizado um container de DI (Unity, Autofac, etc.) para manter o projeto mais simples dado o prazo; as instâncias são compostas manualmente no construtor de cada Controller.
- **Tratamento de erros por tipo de exceção** — em vez de um filtro de exceção global, cada Controller usa blocos `catch` por tipo (`ArgumentException` → 400, `KeyNotFoundException` → 404, `InvalidOperationException` → 409), mantendo o fluxo de erro explícito e fácil de acompanhar.

## Estrutura de pastas

```
TesteEscolaAPI/
├── Controllers/            # Endpoints da API (AlunosController, TurmasController, MatriculasController, RelatoriosController)
├── Services/
│   └── Interfaces/         # Regras de negócio
├── Repositories/
│   └── Interfaces/         # Acesso a dados via Dapper
├── DTOs/
│   ├── Requests/           # Contratos de entrada (Create/Update)
│   └── Responses/          # Contratos de saída
├── Models/                 # Entidades espelhando as tabelas (Aluno, Turma, Matricula)
├── Infrastructure/         # DbConnectionFactory / IDbConnectionFactory
├── App_Start/
│   └── WebApiConfig.cs     # Configuração de rotas (attribute routing habilitado)
└── Web.config              # Connection string e configuração do ASP.NET

TesteUnitarioEscolaAPI.Tests/
└── MatriculaServiceTests.cs   # Testes unitários da regra de matrícula (MSTest + Moq)
```

## Pré-requisitos

- Visual Studio 2022 ou superior, com a workload **ASP.NET and web development** instalada
- Componente **.NET Framework project and item templates** e **.NET Framework 4.8 targeting pack** (Visual Studio Installer → Individual Components)
- **SQL Server LocalDB** (geralmente já vem com o Visual Studio) ou SQL Server Express
- **SQL Server Management Studio (SSMS)** ou o SQL Server Object Explorer do próprio Visual Studio, para rodar o script de criação do banco
- Postman (ou similar) para testar os endpoints manualmente

## Como rodar o projeto

### 1. Criar o banco de dados

O arquivo `script-banco.sql` (fornecido junto com o enunciado do teste) cria o banco `TesteEscola`, as tabelas `Aluno`, `Turma` e `Matricula`, e já popula dados de exemplo.

1. Abra o SSMS (ou o SQL Server Object Explorer do Visual Studio) e conecte no servidor `(localdb)\MSSQLLocalDB`
2. Abra o arquivo `script-banco.sql`
3. Execute com `F5` — o próprio script cuida da criação do banco (`IF DB_ID('TesteEscola') IS NULL CREATE DATABASE...`), não é necessário criar o banco manualmente antes
4. Confirme, no Object Explorer, que as tabelas `Aluno`, `Turma` e `Matricula` foram criadas dentro do banco `TesteEscola`, já com os dados de exemplo

### 2. Configurar a connection string

Já está configurada no `Web.config` do projeto, apontando para o LocalDB:

```xml
<connectionStrings>
  <add name="DefaultConnection"
       connectionString="Server=(localdb)\MSSQLLocalDB;Database=TesteEscola;Trusted_Connection=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

Se estiver usando SQL Server Express ou uma instância nomeada diferente, ajuste o valor de `Server` de acordo com o seu ambiente.

### 3. Restaurar pacotes e rodar

1. Abra `TesteEscolaAPI.slnx` no Visual Studio
2. Aguarde a restauração automática dos pacotes NuGet (ou clique com o botão direito na Solution → **Restore NuGet Packages**)
3. Defina `TesteEscolaAPI` como projeto de inicialização (já deve vir configurado por padrão)
4. Pressione `F5` (ou `Ctrl+F5` para rodar sem debug) — o IIS Express vai subir a API, geralmente em `https://localhost:{porta}`
5. A porta exata aparece na barra de endereço do navegador que abre automaticamente

## Endpoints

### Alunos

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/alunos` | Lista paginada de alunos, com filtro opcional por nome (`?nome=`) e paginação (`?pagina=`, `?tamanhoPagina=`) |
| `GET` | `/api/alunos/{id}` | Busca um aluno pelo Id |
| `POST` | `/api/alunos` | Cria um novo aluno |
| `PUT` | `/api/alunos/{id}` | Atualiza os dados de um aluno |
| `DELETE` | `/api/alunos/{id}` | Exclusão lógica (`Ativo = 0`) — o registro não é removido do banco |

**Exemplo — corpo do `POST /api/alunos`:**
```json
{
  "nome": "Fulano da Silva",
  "email": "fulano.silva@email.com",
  "dataNascimento": "2006-04-10"
}
```

**Exemplo — resposta paginada do `GET /api/alunos?pagina=1&tamanhoPagina=2`:**
```json
{
  "total": 8,
  "pagina": 1,
  "tamanhoPagina": 2,
  "itens": [
    { "id": 1, "nome": "Ana Souza", "email": "ana.souza@email.com", "dataNascimento": "2006-03-14", "ativo": true },
    { "id": 2, "nome": "Bruno Lima", "email": "bruno.lima@email.com", "dataNascimento": "2005-11-02", "ativo": true }
  ]
}
```

### Turmas

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/turmas` | Lista todas as turmas, com o total de vagas e as vagas disponíveis de cada uma |

### Matrículas

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/matriculas` | Matricula um aluno em uma turma, validando as regras de negócio dentro de uma transação |

**Corpo do `POST /api/matriculas`:**
```json
{
  "alunoId": 1,
  "turmaId": 1
}
```

**Resposta (201 Created):**
```json
{
  "id": 9,
  "alunoId": 1,
  "turmaId": 1,
  "dataMatricula": "2026-09-04T10:00:00",
  "vagasRestantes": 27
}
```

### Relatórios

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/relatorios/alunos-por-turma` | Retorna, por turma, o nome, a quantidade de alunos matriculados e as vagas restantes |

**Resposta:**
```json
[
  { "nomeTurma": "3A - Ensino Medio", "quantidadeAlunosMatriculados": 2, "vagasRestantes": 28 },
  { "nomeTurma": "3B - Ensino Medio", "quantidadeAlunosMatriculados": 0, "vagasRestantes": 30 },
  { "nomeTurma": "Turma Intensiva", "quantidadeAlunosMatriculados": 4, "vagasRestantes": 1 },
  { "nomeTurma": "Turma Lotada", "quantidadeAlunosMatriculados": 2, "vagasRestantes": 0 }
]
```

Essa consulta é feita inteiramente em SQL, com `LEFT JOIN` entre `Turma` e `Matricula` e `GROUP BY` — o `LEFT JOIN` garante que turmas sem nenhuma matrícula ainda apareçam no relatório com contagem zero, em vez de serem excluídas do resultado.

## Regras de negócio da matrícula

O `POST /api/matriculas` é o endpoint mais sensível do teste. Todas as validações abaixo, mais a gravação, acontecem **dentro de uma única transação** — se qualquer uma falhar, nada é persistido:

1. **O aluno precisa existir e estar ativo** — caso contrário, `404` (não encontrado) ou `409` (inativo)
2. **A turma precisa existir e ter vaga disponível** — `404` ou `409`
3. **O aluno não pode já estar matriculado na mesma turma** — `409`
4. **Gravação atômica** — o `INSERT` na tabela `Matricula` e o decremento de `VagasDisponiveis` na tabela `Turma` ocorrem na mesma transação; se qualquer etapa falhar, um `Rollback()` desfaz tudo

O decremento de vaga é feito com um `UPDATE` condicional (`WHERE VagasDisponiveis > 0`), que funciona como a validação definitiva de vaga contra condições de corrida — mesmo que duas requisições simultâneas leiam a mesma turma com vaga disponível, apenas uma delas consegue de fato decrementar; a outra recebe `409`.

## Testando com o Postman

Os dados de exemplo do `script-banco.sql` já cobrem os principais cenários de erro da matrícula, sem precisar cadastrar nada manualmente:

| Cenário | Como testar | Resultado esperado |
|---|---|---|
| Matrícula com sucesso | `POST /api/matriculas` com um aluno ativo em uma turma com vaga (ex: `alunoId: 1`, `turmaId: 1`) | `201 Created` |
| Turma sem vaga | `turmaId: 4` (**Turma Lotada**, 0 vagas disponíveis) | `409 Conflict` |
| Aluno inativo | `alunoId: 4` (**Diego Ferreira**, `Ativo = 0`) | `409 Conflict` |
| Matrícula duplicada | `alunoId: 2`, `turmaId: 1` (Bruno Lima já matriculado na turma 1) | `409 Conflict` |
| Aluno inexistente | `alunoId: 999` | `404 Not Found` |
| Turma inexistente | `turmaId: 999` | `404 Not Found` |

## Testes unitários

O bônus de testes unitários foi implementado cobrindo a regra de negócio da matrícula, no projeto `TesteUnitarioEscolaAPI.Tests` (MSTest + Moq).

**Estratégia de teste:** como o `MatriculaService` depende de `IDbConnectionFactory`, `IAlunoRepository`, `ITurmaRepository` e `IMatriculaRepository` — todas interfaces — os testes mockam essas dependências com Moq, incluindo `IDbConnection`/`IDbTransaction` (interfaces do próprio `System.Data`). Isso permite testar toda a lógica de validação e orquestração da transação **sem depender de um banco de dados real**.

Cenários cobertos:

- Matrícula realizada com sucesso (commit da transação e retorno correto)
- Aluno não encontrado
- Aluno inativo
- Turma não encontrada
- Turma sem vaga disponível
- Aluno já matriculado na turma

Para rodar: abra o **Test Explorer** no Visual Studio (`Ctrl+E, T`) e clique em **Run All Tests**.

## Decisões técnicas e justificativas

- **Instanciação manual de dependências nos Controllers** (sem container de DI) — decisão deliberada para reduzir a superfície de configuração do projeto dado o prazo de 3 dias, mantendo ainda assim a inversão de dependência via interfaces (o que permite testar cada camada isoladamente).
- **Tratamento de erro via `try/catch` por tipo de exceção**, em vez de um filtro de exceção global (`ExceptionFilterAttribute`) — escolha consciente de simplicidade: o mapeamento de exceção para status HTTP fica explícito em cada Controller, sem uma camada adicional.
- **Repositórios "transaction-aware"** (`GetByIdComTransacao`, `DecrementarVaga`, `ExisteAlunoNaTurma`, `InsertMatricula`) recebem `IDbConnection`/`IDbTransaction` como parâmetro, em vez de abrirem sua própria conexão — necessário para que a leitura de validação e a escrita da matrícula participem da mesma transação, garantindo atomicidade.
- **`SCOPE_IDENTITY()`**, e não `@@IDENTITY`, para recuperar o Id gerado após um `INSERT` — evita capturar um identity gerado por um trigger em outra tabela, ficando restrito ao escopo da própria instrução.
- **Paginação via `OFFSET/FETCH`** no SQL Server, com validação de `pagina`/`tamanhoPagina` no Service (valores inválidos ou fora de um teto de 100 caem para um valor padrão, em vez de gerar erro) — parâmetros de paginação são tratados como preferência do cliente, não como contrato rígido.
- **DTOs de entrada não expõem `Id`, `Ativo` ou `DataCadastro`** — esses campos são responsabilidade do servidor/banco, não do cliente da API.

## Possíveis evoluções

- Adicionar um container de Injeção de Dependência (ex: Unity, Autofac) para eliminar a instanciação manual nos Controllers
- Adicionar testes unitários para `AlunoService` e `TurmaService`
- Adicionar testes de integração contra uma instância real (ou containerizada) do SQL Server
- Adicionar um filtro de exceção global, caso o projeto cresça e o `try/catch` repetido por Controller se torne difícil de manter
- Adição de Unit of Work: optei por não introduzir uma abstração formal de Unit of Work neste projeto. O MatriculaService já implementa o princípio central do padrão de forma direta, porém Caso o sistema evoluísse para incluir outras operações transacionais a extração de um Unit of Work formal se justificaria, centralizando a abertura de conexão/transação e evitando repetição entre os Services
- Validação de entrada via Data Annotations pois hoje a validação dos DTOs é feita manualmente dentro dos Services. Adicionar atributos como [Required], [EmailAddress] e [Range] diretamente nos DTOs, combinados com a checagem de ModelState.IsValid nos Controllers, centralizaria essa validação na camada de entrada da API, deixando os Services focados só nas regras de negócio.
- Autenticação via JWT, o teste não exigiu autenticação, então nenhum endpoint está protegido atualmente. Numa evolução real do projeto, seria adicionado um mecanismo de emissão e validação de tokens JWT (via System.IdentityModel.Tokens.Jwt ou similar), com o atributo [Authorize] protegendo os endpoints sensíveis (criação/edição/exclusão de alunos e matrículas), mantendo talvez as listagens (GET) públicas.
- Configuração de CORS, como a API não expõe atualmente nenhuma política de CORS, um consumidor front-end rodando em outro domínio não conseguiria chamar os endpoints diretamente do navegador. Isso seria resolvido habilitando o CORS no WebApiConfig.cs (pacote Microsoft.AspNet.WebApi.Cors), restringindo as origens permitidas conforme o ambiente.