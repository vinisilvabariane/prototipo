using IrisWeb.Application.Interfaces.ModelCatalog;
using IrisWeb.Infrastructure.Storage;

namespace IrisWeb.Tests.Infrastructure;

public sealed class FileSystemModelArtifactStorageTests : IDisposable
{
    private const string Chave = "facenet/1.0.0/model-package.zip";

    private readonly string _raiz = Path.Combine(
        Path.GetTempPath(), "iris-tests", "artifacts", Guid.NewGuid().ToString("N"));

    private FileSystemModelArtifactStorage Storage() =>
        new(new ModelStorageOptions { Path = _raiz });

    public void Dispose()
    {
        if (Directory.Exists(_raiz))
            Directory.Delete(_raiz, recursive: true);
    }

    private static MemoryStream Bytes(params byte[] conteudo) => new(conteudo);

    private static async Task<byte[]> LerTudo(Stream stream)
    {
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, CancellationToken.None);
        return buffer.ToArray();
    }

    [Fact]
    public void O_construtor_cria_a_raiz_que_ainda_nao_existe()
    {

        Assert.False(Directory.Exists(_raiz));

        _ = Storage();

        Assert.True(Directory.Exists(_raiz));
    }

    [Fact]
    public async Task Save_e_OpenRead_preservam_o_conteudo()
    {
        var storage = Storage();
        var conteudo = new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x00, 0xFF, 0x0D, 0x0A, 0x1A };

        await storage.SaveAsync(Chave, Bytes(conteudo), CancellationToken.None);

        await using var lido = await storage.OpenReadAsync(Chave, CancellationToken.None);

        Assert.NotNull(lido);
        Assert.Equal(conteudo, await LerTudo(lido));
    }

    [Fact]
    public async Task OpenRead_de_chave_inexistente_devolve_null()
    {
        var storage = Storage();

        Assert.Null(await storage.OpenReadAsync(Chave, CancellationToken.None));
        Assert.Null(await storage.GetMetadataAsync(Chave, CancellationToken.None));
    }

    [Fact]
    public async Task GetMetadata_bate_com_o_tamanho_gravado()
    {
        var storage = Storage();
        var conteudo = new byte[4096];
        Random.Shared.NextBytes(conteudo);

        await storage.SaveAsync(Chave, Bytes(conteudo), CancellationToken.None);

        var metadata = await storage.GetMetadataAsync(Chave, CancellationToken.None);

        Assert.NotNull(metadata);
        Assert.Equal(conteudo.Length, metadata.Size);
        Assert.False(string.IsNullOrWhiteSpace(metadata.ETag));
    }

    [Fact]
    public async Task Save_por_cima_de_chave_existente_troca_o_conteudo_inteiro()
    {
        var storage = Storage();

        await storage.SaveAsync(Chave, Bytes(1, 1, 1, 1, 1, 1, 1, 1), CancellationToken.None);
        await storage.SaveAsync(Chave, Bytes(2, 2), CancellationToken.None);

        await using var lido = await storage.OpenReadAsync(Chave, CancellationToken.None);

        Assert.NotNull(lido);
        Assert.Equal<byte[]>([2, 2], await LerTudo(lido));
    }

    [Fact]
    public async Task Save_interrompido_no_meio_nao_deixa_arquivo_no_destino()
    {
        var storage = Storage();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            storage.SaveAsync(Chave, new StreamQueCaiNoMeio(), CancellationToken.None));

        Assert.Null(await storage.OpenReadAsync(Chave, CancellationToken.None));

        Assert.Empty(Directory.GetFiles(_raiz, "*", SearchOption.AllDirectories));
    }

    [Theory]
    [InlineData("../fora-da-raiz.zip")]
    [InlineData("facenet/../../fora-da-raiz.zip")]
    [InlineData("..")]
    [InlineData("/etc/passwd")]
    [InlineData("//servidor/share/pacote.zip")]
    [InlineData(@"C:\Windows\System32\drivers\etc\hosts")]
    [InlineData(@"..\fora-da-raiz.zip")]
    [InlineData(@"facenet\1.0.0\model-package.zip")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Chave_hostil_e_recusada_sem_escrever_nem_ler_fora_da_raiz(string chave)
    {
        var storage = Storage();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            storage.SaveAsync(chave, Bytes(1, 2, 3), CancellationToken.None));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            storage.OpenReadAsync(chave, CancellationToken.None));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            storage.GetMetadataAsync(chave, CancellationToken.None));

        Assert.Empty(Directory.GetFileSystemEntries(_raiz));
        Assert.False(File.Exists(Path.Combine(Directory.GetParent(_raiz)!.FullName, "fora-da-raiz.zip")));
    }

    [Fact]
    public async Task Chave_hostil_nao_e_confundida_com_indisponibilidade()
    {
        var storage = Storage();

        var excecao = await Assert.ThrowsAsync<ArgumentException>(() =>
            storage.OpenReadAsync("../fora.zip", CancellationToken.None));

        Assert.IsNotType<ModelArtifactStorageException>(excecao);
    }

    private sealed class StreamQueCaiNoMeio : Stream
    {
        private int _lidos;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => _lidos;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_lidos > 0)
                throw new InvalidOperationException("A conexao caiu no meio do upload.");

            _lidos += count;
            return count;
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
