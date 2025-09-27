# Configuração do AWS SES para recuperação de senha

Este guia descreve o que precisa ser feito na conta AWS para permitir que a API envie e-mails de recuperação de senha usando o Amazon Simple Email Service (SES).

## 1. Verifique domínio ou endereço remetente
1. Acesse o console do [Amazon SES](https://console.aws.amazon.com/ses/).
2. No menu lateral, escolha **Verified Identities** e clique em **Create identity**.
3. Selecione **Domain** (recomendado) e informe o domínio que será utilizado como remetente (por exemplo `example.com`).
4. Copie os registros DNS apresentados e publique-os no provedor DNS do domínio (Route53, Cloudflare, etc.).
5. Aguarde a validação do domínio. Enquanto o domínio não estiver verificado, os envios serão bloqueados.
6. Caso não seja possível validar o domínio, é possível verificar apenas um endereço de e-mail específico escolhendo **Email address** e seguindo o fluxo de verificação.

> O endereço configurado em `Email:AwsSes:FromAddress` deve pertencer ao domínio/endereço verificado nesta etapa.

## 2. Solicite saída do sandbox (opcional)
Por padrão, novas contas do SES ficam no modo sandbox e só conseguem enviar e-mails para endereços verificados. Para liberar envios para qualquer destinatário:

1. No console AWS, acesse o [Support Center](https://console.aws.amazon.com/support/home#/) e abra um **Service quota increase** para o SES.
2. Na solicitação, informe a região utilizada, explique o caso de uso (recuperação de senha da aplicação Parking) e peça para sair do sandbox.
3. Aguarde a aprovação. Enquanto o sandbox estiver ativo, limite os testes a endereços verificados.

## 3. Crie usuário IAM com permissão para o SES
1. No console AWS, abra o serviço **IAM** e clique em **Users** > **Add users**.
2. Defina um nome (ex.: `parking-api-ses`), marque **Access key - Programmatic access** e avance.
3. Anexe a política gerenciada `AmazonSESFullAccess` ou crie uma política restrita que permita `ses:SendEmail` e `ses:SendRawEmail` na região necessária.
4. Finalize a criação e guarde o `Access key ID` e `Secret access key` em local seguro.

## 4. Configure as credenciais na aplicação
Você pode optar por usar as chaves geradas na etapa anterior ou uma role do EC2/ECS/EKS. As duas abordagens estão suportadas:

- **Chaves dedicadas**: defina as variáveis de ambiente antes de subir a API:
  ```bash
  export PARKING__EMAIL__AWSSES__ACCESSKEYID="AKIA..."
  export PARKING__EMAIL__AWSSES__SECRETACCESSKEY="<segredo>"
  export PARKING__EMAIL__AWSSES__REGION="us-east-1"
  export PARKING__EMAIL__AWSSES__FROMADDRESS="no-reply@seu-dominio.com"
  ```
  Os nomes seguem o padrão do .NET (`__` substitui `:`). Caso prefira JSON, atualize `appsettings.{Environment}.json` com os mesmos campos.

- **Credenciais padrão da AWS**: deixe `AccessKeyId` e `SecretAccessKey` vazios em `appsettings.json`. O SDK vai procurar credenciais no ambiente (profiles em `~/.aws/credentials`, variables padrão `AWS_ACCESS_KEY_ID`, roles, etc.).

> Nunca faça commit das chaves de acesso no repositório. Use **Secret Manager**, variáveis de ambiente ou o provedor de configurações que preferir.

## 5. Ajuste a URL de redefinição de senha
Atualize o campo `PasswordReset:ResetUrl` com a URL da página do frontend que consumirá o token enviado por e-mail. Exemplo:

```json
"PasswordReset": {
  "TokenExpirationMinutes": 60,
  "ResetUrl": "https://app.seudominio.com/redefinir-senha"
}
```

A API sempre acrescentará `?token=<valor>` (ou `&token=...` caso já exista query string) à URL configurada.

## 6. Teste o envio
Com todas as etapas configuradas:

1. Execute a API (`dotnet run` ou `docker compose up`).
2. Faça um POST para `/api/Auth/forgot-password` passando o e-mail cadastrado no sistema.
3. Verifique no console do SES (em **Event publishing**) ou na caixa de entrada se o e-mail foi recebido. Caso haja falhas, consulte o CloudWatch Logs do SES ou ajuste a Configuration Set informada em `Email:AwsSes:ConfigurationSetName`.

Seguindo os passos acima o serviço de recuperação de senha estará apto a enviar mensagens através do AWS SES.
