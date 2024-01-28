using Tracer.Contracts;

namespace ZoNo.Activation
{
  // Extend this class to implement new ActivationHandlers. See DefaultActivationHandler for an example.
  // https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/activation.md
  public abstract class ActivationHandler<T>(
    ITraceFactory traceFactory) : IActivationHandler where T : class
  {
    private readonly ITraceFactory _traceFactory = traceFactory;

    // Override this method to add the logic for whether to handle the activation.
    protected virtual bool CanHandleInternal(T args)
    {
      using var trace = _traceFactory.CreateNew();
      return true;
    }

    // Override this method to add the logic for your activation handler.
    protected abstract Task HandleInternalAsync(T args);

    public bool CanHandle(object args)
    {
      using var trace = _traceFactory.CreateNew();
      return args is T && CanHandleInternal(args as T);
    }

    public async Task HandleAsync(object args)
    {
      using var trace = _traceFactory.CreateNew();
      await HandleInternalAsync(args as T);
    }
  }
}