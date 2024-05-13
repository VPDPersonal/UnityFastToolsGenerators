using System;
using System.Text;

namespace UnityFastToolsGenerators.Helpers.Code;

public class CodeWriter : IDisposable
{
    private readonly StringBuilder _buffer;
    private readonly int _minIndentLevel;
    private int _indentLevel;
    
    public int Length
    {
        get => _buffer.Length;
        set => _buffer.Length = value;
    }
    
    public CodeWriter(int minIndentLevel = 0)
    {
        _buffer = new StringBuilder();
        
        _indentLevel = minIndentLevel;
        _minIndentLevel = minIndentLevel;
    }
    
    public CodeWriter Append(string value = "")
    {
        _buffer.Append(value);
        return this;
    }
    
    public CodeWriter AppendLine(string value = "")
    {
        if (string.IsNullOrEmpty(value)) _buffer.AppendLine();
        else _buffer.AppendLine($"{new string(' ', _indentLevel * 4)}{value}");
        
        return this;
    }
    
    public IDisposable BeginIndentScope() => 
        new IndentScope(this);
    
    public IDisposable BeginBlockScope() => 
        new BlockScope(this);
    
    public CodeWriter BeginBlock()
    {
        AppendLine("{");
        IncreaseIndent();
        
        return this;
    }
    
    public CodeWriter EndBlock()
    {
        DecreaseIndent();
        AppendLine("}");
        
        return this;
    }
    
    public CodeWriter IncreaseIndent()
    {
        _indentLevel++;
        return this;
    }
    
    public CodeWriter DecreaseIndent()
    {
        if (_indentLevel <= 0) return this;
        _indentLevel--;
        
        return this;
    }
    
    public void Clear()
    {
        _buffer.Clear();
        _indentLevel = _minIndentLevel;
    }
    
    public override string ToString() =>
        _buffer.ToString();
    
    public void Dispose() =>
        Clear();
    
    private readonly struct IndentScope : IDisposable
    {
        private readonly CodeWriter _source;
        
        public IndentScope(CodeWriter source)
        {
            _source = source;
            source.IncreaseIndent();
        }
        
        public void Dispose() =>
            _source.DecreaseIndent();
    }
    
    private readonly struct BlockScope : IDisposable
    {
        private readonly CodeWriter _source;
        
        public BlockScope(CodeWriter source)
        {
            _source = source;
            source.BeginBlock();
        }
        
        public void Dispose() =>
            _source.EndBlock();
    }
}