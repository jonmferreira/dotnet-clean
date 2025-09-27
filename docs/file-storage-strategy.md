# Estratégia para salvar e disponibilizar arquivos

## O que já existe hoje

A API não persiste arquivos gerados: o endpoint `GET /api/tickets/export/pdf` chama `TicketPdfExporter.Generate` e devolve o conteúdo diretamente com `File(...)`. Nenhuma cópia é salva em disco ou em um storage externo. Além disso, o exportador apenas exige que as imagens de _background_ estejam presentes na pasta `Assets` local antes de montar o PDF.

## Como implementar persistência de arquivos

1. **Criar um serviço de storage**
   - Defina uma abstração (`IFileStorageService`) na camada de aplicação que permita salvar, recuperar e remover arquivos.
   - Forneça implementações na infraestrutura conforme a necessidade (sistema de arquivos local, Amazon S3, Azure Blob Storage etc.).
2. **Persistir metadados**
   - Adicione uma entidade/registro (por exemplo, `StoredFile`) com informações como nome lógico, caminho/URL, `ContentType`, tamanho e data de expiração.
   - Armazene o identificador desse registro junto ao agregado que precisa referenciar o arquivo (por exemplo, o ticket ou o checklist de inspeção).
3. **Salvar antes de disponibilizar**
   - Ao gerar o PDF (ou receber um upload), salve o arquivo por meio do serviço de storage.
   - Guarde os metadados e retorne apenas um identificador para o cliente.
4. **Disponibilizar o arquivo**
   - Crie um endpoint que receba o identificador, recupere os metadados e use o serviço de storage para baixar o conteúdo.
   - Retorne uma `FileResult` com o `ContentType` correto; para storages externos, considere devolver uma URL pré-assinada.
5. **Expiração e limpeza**
   - Se o arquivo for temporário (por exemplo, relatórios pontuais), agende um processo para remover objetos expirados do storage e da base de metadados.

## Boas práticas adicionais

- Limite o tamanho máximo permitido por upload/geração e valide o `ContentType` esperado.
- Proteja os endpoints de download com autorização e auditoria.
- Para storages externos, configure _lifecycle rules_ (por exemplo, Glacier/S3 IA) e criptografia em repouso.
- Versione os contratos de resposta para indicar quando um arquivo está pronto, ainda sendo processado ou expirado.
