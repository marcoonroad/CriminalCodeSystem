CriminalCodeSystem
==================

Projeto Web API para gerenciar códigos penais.

## Configuração

Criar arquivo `CriminalCodeSystem/credentials.json` no seguinte formato.

```json
{
    "MASTER_JWT_SECRET": "<SECRET>",
    "MASTER_ACCESS_TOKEN": "<TOKEN>",
    "MYSQL_DB_CONNECTION": "server=<DBHOST>;user=<DBUSER>;password=<DBPASS>;database=<DBNAME>"
}
```

Se este arquivo não existir ou se o valor da chave `"MYSQL_DB_CONNECTION"` estiver vazio, a aplicação irá assumir um _storage_ em memória (como é o caso dos testes).
É recomendado que ambos `"MASTER_ACCESS_TOKEN"` e `"MASTER_JWT_SECRET"` sejam uma cadeia de caracteres aleatórias e longas o suficiente para impedir qualquer tipo de
ataque que iria tentar descobrir os por força bruta. Tais segredos podem serem gerados com o seguinte comando unix bash para cada:

```bash
tr -cd '[:alnum:]' < /dev/urandom | fold -w36 | head -n 1 # segredo aleatório criptograficamente forte de 36 caracteres
```

## Migrações

```bash
make migrations-vendor
make migrations-create
make migrations-perform
```

Remover estas com `dotnet ef migrations remove`.

## Execução

```bash
make run
```

## Testes

```
make test
```

## Endpoints

Esta API expõe os seguintes contextos:

- `/BackOffice`, prefixo de rota para acesso interno (API não-pública) via token mestre
    +  **POST** `/BackOffice/RegisterNewUser`, endpoint interno para criar novos usuários
    +  **POST** `/BackOffice/FlushTokens`, endpoint interno para limpar tokens desabilitados que já se encontram expirados
- `/User`, prefixo de rota para o usuário controlar e gerenciar seus acessos
    + **POST** `/User/Login`, endpoint para o usuário se autenticar e gerar um token JWT
    + **POST** `/User/ChangePassword`, endpoint para o usuário alterar sua senha (necessita estar logado, i.e, passar o token JWT gerado anteriormente)
    + **POST** `/User/Logoff`, endpoint para o usuário terminar sua sessão ativa (i.e, não-expirada)
- `/CriminalCode`, prefixo de rota para gerir os códigos penais cadastrados
    + **GET** `/CriminalCode/List`, endpoint para filtrar e paginar códigos penais
    + **POST** `/CriminalCode/Add`, endpoint para adicionar novos códigos penais
    + **DELETE** `/CriminalCode/Exclude`, endpoint para remover códigos penais existentes
    + **PUT** `/CriminalCode/Edit`, endpoint para alterar códigos penais existentes
    + **GET** `/CriminalCode/Find`, endpoint para encontrar um código penal em específico

Mais detalhes sobre os endpoints disponibilizados (como o que precisam e o que retornam) podem serem encontrados na rota `/swagger` ao rodar localmente
e abrir no navegador.

## Problemas Comuns

Ao executar o cliente MySQL dentro do WSL apontando para um servidor MySQL no Windows, é necessário confirmar que a porta foi devidamente exposta pelo firewall (rodar via Administrador no Powershell):

```powershell
netsh advfirewall firewall add rule name=WSL_MYSQL dir=in protocol=tcp action=allow localport=3306 remoteip=localsubnet profile=any
```

E então, pegar o endereço para o banco por um dos comandos abaixo (rodar no Bash dentro do WSL):

```bash
echo $(hostname).local
awk '/nameserver/ { print $2 }' /etc/resolv.conf
```

A API assume um banco existente com encoding UTF-8, se houver problemas de caracteres estranhos sendo passados, uma alternativa pode ser criar um novo banco em UTF-8:

```sql
CREATE DATABASE newappdb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```
