// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bicep.Core.Diagnostics;
using Bicep.Core.Syntax;

namespace Bicep.Core.Semantics
{
    /// <summary>
    /// Represents a language scope that declares local symbols. (For example the item or index variables in loops are local symbols.)
    /// </summary>
    public class LocalScope : Symbol, ILanguageScope
    {
        public LocalScope(string name, SyntaxBase enclosingSyntax, IEnumerable<LocalVariableSymbol> locals, IEnumerable<LocalScope> childScopes)
            : base(name)
        {
            this.EnclosingSyntax = enclosingSyntax;
            this.Locals = locals.ToImmutableArray();
            this.ChildScopes = childScopes.ToImmutableArray();
        }

        public SyntaxBase EnclosingSyntax { get; }

        public ImmutableArray<LocalVariableSymbol> Locals { get; }

        public ImmutableArray<LocalScope> ChildScopes { get; }

        public override void Accept(SymbolVisitor visitor) => visitor.VisitLocalScope(this);

        public override SymbolKind Kind => SymbolKind.Scope;

        public override IEnumerable<Symbol> Descendants => this.ChildScopes.Concat<Symbol>(this.Locals);

        public LocalScope ReplaceChildren(IEnumerable<LocalScope> newChildren) => new(this.Name, this.EnclosingSyntax, this.Locals, newChildren);

        public IEnumerable<DeclaredSymbol> GetDeclarationsByName(string name) => this.Locals.Where(symbol => symbol.NameSyntax.IsValid && string.Equals(symbol.Name, name, LanguageConstants.IdentifierComparison)).ToList();
        
        public IEnumerable<DeclaredSymbol> AllDeclarations => this.Locals;

        public override IEnumerable<ErrorDiagnostic> GetDiagnostics()
        {
            // TODO: Remove when loops codegen is done.
            yield return DiagnosticBuilder.ForPosition(((ForSyntax) this.EnclosingSyntax).ForKeyword).LoopsNotSupported();
        }
    }
}