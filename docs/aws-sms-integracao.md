# Integração com Envio de SMS da AWS (Amazon SNS)

Este guia descreve passo a passo como configurar e enviar SMS usando o Amazon Simple Notification Service (SNS). Os passos assumem que você já possui uma conta AWS ativa e privilégios para criar recursos IAM e SNS.

## 1. Preparação do Ambiente
1. **Crie/acesse uma conta AWS** e faça login no [Console de Gerenciamento AWS](https://console.aws.amazon.com/).
2. **Defina a região adequada**: escolha uma região suportada para SMS (ex.: `us-east-1`). Algumas regiões limitam recursos de SMS; consulte a [documentação da AWS](https://docs.aws.amazon.com/sns/latest/dg/sns-supported-regions-countries.html).
3. **Habilite o envio de SMS**: em **Amazon SNS > Text messaging (SMS)** ajuste os limites de gasto e solicite elevação de limites se necessário.

## 2. Configuração de Permissões (IAM)
1. No console AWS, acesse **IAM > Users** e crie um usuário dedicado ao envio de SMS.
2. Selecione **Programmatic access** para gerar Access Key e Secret Key.
3. Anexe a política gerenciada `AmazonSNSFullAccess` ou crie uma política personalizada com permissões mínimas necessárias (`sns:Publish`, `sns:CheckIfPhoneNumberIsOptedOut`, etc.).
4. Salve as credenciais geradas (Access Key ID e Secret Access Key) para configurar no seu aplicativo.

## 3. Configuração via AWS CLI (opcional)
1. Instale a [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html).
2. Configure com `aws configure` e informe Access Key, Secret Key, região e formato de saída.
3. Teste o envio com o comando:
   ```bash
   aws sns publish \
     --phone-number "+5511999999999" \
     --message "Mensagem de teste via Amazon SNS"
   ```
   Substitua pelo número no formato E.164 (código do país + número completo).

## 4. Integração em Aplicações (.NET Exemplo)
1. Adicione o pacote NuGet `AWSSDK.SimpleNotificationService`:
   ```bash
   dotnet add package AWSSDK.SimpleNotificationService
   ```
2. Configure as credenciais (ex.: via `appsettings.json`, variáveis de ambiente ou `AWS SDK Credential Store`).
3. Código exemplo:
   ```csharp
   using Amazon;
   using Amazon.SimpleNotificationService;
   using Amazon.SimpleNotificationService.Model;

   var snsClient = new AmazonSimpleNotificationServiceClient(
       awsAccessKeyId: "ACCESS_KEY",
       awsSecretAccessKey: "SECRET_KEY",
       RegionEndpoint.USEast1);

   var request = new PublishRequest
   {
       Message = "Sua mensagem",
       PhoneNumber = "+5511999999999"
   };

   await snsClient.PublishAsync(request);
   ```
4. Em produção, substitua credenciais hardcoded por provedores seguros (IAM Role, Secrets Manager, Parameter Store, etc.).

## 5. Boas Práticas e Considerações
- **Opt-in/Opt-out**: garanta consentimento do destinatário e implemente mecanismos de opt-out.
- **Limites de gastos**: monitore o limite em **SNS > Text messaging (SMS) > Preferences**.
- **Monitoramento**: utilize CloudWatch para acompanhar métricas e alarmes.
- **Mensagens transacionais vs promocionais**: verifique restrições regulatórias do país.
- **Registros e auditoria**: armazene logs de envio e respostas para auditoria e suporte.

## 6. Próximos Passos
- Automatize envio com tópicos SNS e assinaturas SMS.
- Integre com filas (SQS) ou lambdas para fluxos assíncronos.
- Avalie uso de Amazon Pinpoint para campanhas de marketing e analytics avançados.

## 7. Endpoint de Verificação na Parking API
Com as dependências adicionadas ao projeto, a API passa a expor o endpoint protegido `POST /api/sms/check`.

1. **Configuração**: defina a região e (opcionalmente) credenciais em `appsettings.json` ou via variáveis de ambiente:
   ```json
   "Aws": {
     "Sms": {
       "Region": "us-east-1",
       "SenderId": "Parking",
       "SmsType": "Transactional"
     }
   }
   ```
   > Se `AccessKey` e `SecretKey` não forem informados, o SDK utilizará os provedores padrão (variáveis `AWS_*`, perfil local, IAM Role, etc.).
2. **Requisição**: envie um POST autenticado com o corpo:
   ```json
   {
     "phoneNumber": "+5511999999999",
     "message": "Mensagem de teste do estacionamento."
   }
   ```
3. **Resposta esperada**: `202 Accepted` indicando que a mensagem foi encaminhada para publicação via SNS. Erros de integração retornam `502 Bad Gateway` com detalhes.

Seguindo estes passos, você terá a configuração necessária para enviar SMS via AWS SNS em ambientes de desenvolvimento ou produção.
