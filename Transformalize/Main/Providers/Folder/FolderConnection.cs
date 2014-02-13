using Transformalize.Configuration;

namespace Transformalize.Main.Providers.Folder {
    public class FolderConnection : AbstractConnection {
        public override string UserProperty { get { return string.Empty; } }
        public override string PasswordProperty { get { return string.Empty; } }
        public override string PortProperty { get { return string.Empty; } }
        public override string DatabaseProperty { get { return string.Empty; } }
        public override string ServerProperty { get { return string.Empty; } }
        public override string TrustedProperty { get { return string.Empty; } }
        public override string PersistSecurityInfoProperty {
            get { return string.Empty; }
        }

        public FolderConnection(Process process, ConnectionConfigurationElement element, AbstractConnectionDependencies dependencies)
            : base(element, dependencies) {

            TypeAndAssemblyName = process.Providers[element.Provider.ToLower()];

            EntityKeysQueryWriter = new EmptyQueryWriter();
            EntityKeysRangeQueryWriter = new EmptyQueryWriter();
            EntityKeysAllQueryWriter = new EmptyQueryWriter();

        }

    }
}