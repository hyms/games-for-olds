using System;
using Godot;

namespace ClasicosMenteActiva.minigames.word_search;

/// <summary>
///     Traduce eventos de entrada (tacto y ratón) en acciones de selección sobre una
///     cuadrícula. Clase pura: no conoce <see cref="WordCell" /> ni
///     <see cref="WordGridContainer" />, solo posiciones locales y callbacks.
///     Se puede probar sin levantar escenas (SRP · plan Fase 3).
/// </summary>
public sealed class BoardInputHandler
{
    private bool _isPressed;

    /// <summary>Se emite al iniciar una selección con la posición local.</summary>
    public event Action<Vector2> SelectionBegan;

    /// <summary>Se emite mientras se arrastra con la posición local actual.</summary>
    public event Action<Vector2> SelectionUpdated;

    /// <summary>Se emite al soltar sobre una posición local válida.</summary>
    public event Action<Vector2> SelectionEnded;

    /// <summary>Se emite al cancelar (dedo/botón suelto sin inicio previo).</summary>
    public event Action SelectionCanceled;

    /// <summary>Procesa un evento de entrada. Devuelve <c>true</c> si lo consumió.</summary>
    public bool Process(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventScreenTouch touch:
                if (touch.Pressed)
                {
                    _isPressed = true;
                    SelectionBegan?.Invoke(touch.Position);
                    return true;
                }

                if (_isPressed)
                {
                    _isPressed = false;
                    SelectionEnded?.Invoke(touch.Position);
                    return true;
                }

                // Release sin pressed previo: estado atrapado, limpiar.
                SelectionCanceled?.Invoke();
                return true;

            case InputEventScreenDrag drag:
                if (!_isPressed) return false;

                SelectionUpdated?.Invoke(drag.Position);
                return true;

            case InputEventMouseButton { ButtonIndex: MouseButton.Left } button:
                if (button.Pressed)
                {
                    _isPressed = true;
                    SelectionBegan?.Invoke(button.Position);
                    return true;
                }

                if (_isPressed)
                {
                    _isPressed = false;
                    SelectionEnded?.Invoke(button.Position);
                    return true;
                }

                SelectionCanceled?.Invoke();
                return true;

            case InputEventMouseMotion motion when _isPressed:
                SelectionUpdated?.Invoke(motion.Position);
                return true;
        }

        return false;
    }

    /// <summary>Cancela cualquier selección en curso y resetea el estado interno.</summary>
    public void Cancel()
    {
        _isPressed = false;
        SelectionCanceled?.Invoke();
    }
}