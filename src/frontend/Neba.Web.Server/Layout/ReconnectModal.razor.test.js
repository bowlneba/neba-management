import { describe, test, expect, beforeEach, afterEach, jest } from '@jest/globals';

describe('ReconnectModal', () => {
  let reconnectModal;
  let retryButton;
  let resumeButton;
  let mockReconnect;
  let mockResumeCircuit;
  let mockReload;

  beforeEach(async () => {
    jest.clearAllMocks();

    // Create mocks for Blazor methods
    mockReconnect = jest.fn();
    mockResumeCircuit = jest.fn();

    // Mock Blazor global
    globalThis.Blazor = {
      reconnect: mockReconnect,
      resumeCircuit: mockResumeCircuit
    };

    // Reset location mock
    mockReload = jest.fn();
    delete globalThis.location;
    globalThis.location = { reload: mockReload };

    // Set up DOM before importing module
    document.body.innerHTML = `
      <dialog id="components-reconnect-modal"></dialog>
      <button id="components-reconnect-button">Retry</button>
      <button id="components-resume-button">Resume</button>
    `;

    reconnectModal = document.getElementById('components-reconnect-modal');
    retryButton = document.getElementById('components-reconnect-button');
    resumeButton = document.getElementById('components-resume-button');

    // Mock dialog methods
    reconnectModal.showModal = jest.fn();
    reconnectModal.close = jest.fn();

    // Reset document visibility
    Object.defineProperty(document, 'visibilityState', {
      writable: true,
      configurable: true,
      value: 'visible'
    });

    // Clear module cache and import to register event listeners
    const modulePath = './ReconnectModal.razor.js';
    if (typeof jest !== 'undefined') {
      jest.resetModules();
    }

    // Import module to register event listeners
    await import(modulePath);
  });

  describe('DOM Elements', () => {
    test('should have reconnect modal element', () => {
      expect(reconnectModal).toBeDefined();
      expect(reconnectModal.id).toBe('components-reconnect-modal');
    });

    test('should have retry button element', () => {
      expect(retryButton).toBeDefined();
      expect(retryButton.id).toBe('components-reconnect-button');
    });

    test('should have resume button element', () => {
      expect(resumeButton).toBeDefined();
      expect(resumeButton.id).toBe('components-resume-button');
    });
  });

  describe('Reconnect state change events', () => {
    test('should show modal when state is "show"', () => {
      // Act
      const event = new CustomEvent('components-reconnect-state-changed', {
        detail: { state: 'show' }
      });
      reconnectModal.dispatchEvent(event);

      // Assert
      expect(reconnectModal.showModal).toHaveBeenCalled();
    });

    test('should close modal when state is "hide"', () => {
      // Act
      const event = new CustomEvent('components-reconnect-state-changed', {
        detail: { state: 'hide' }
      });
      reconnectModal.dispatchEvent(event);

      // Assert
      expect(reconnectModal.close).toHaveBeenCalled();
    });

    test('should reload page when state is "rejected"', () => {
      // Act
      const event = new CustomEvent('components-reconnect-state-changed', {
        detail: { state: 'rejected' }
      });
      reconnectModal.dispatchEvent(event);

      // Assert
      expect(mockReload).toHaveBeenCalled();
    });

    test('should add visibility change listener when state is "failed"', () => {
      // Arrange
      const addEventListenerSpy = jest.spyOn(document, 'addEventListener');

      // Act
      const event = new CustomEvent('components-reconnect-state-changed', {
        detail: { state: 'failed' }
      });
      reconnectModal.dispatchEvent(event);

      // Assert
      expect(addEventListenerSpy).toHaveBeenCalledWith(
        'visibilitychange',
        expect.any(Function)
      );

      addEventListenerSpy.mockRestore();
    });
  });

  describe('Retry button behavior', () => {
    test('should call Blazor.reconnect when retry button is clicked', async () => {
      // Arrange
      mockReconnect.mockResolvedValue(true);

      // Act
      retryButton.click();
      await Promise.resolve();

      // Assert
      expect(mockReconnect).toHaveBeenCalled();
    });

    test('should reload page when reconnect returns false', async () => {
      // Arrange
      mockReconnect.mockResolvedValue(false);
      mockResumeCircuit.mockResolvedValue(false);

      // Act
      retryButton.click();
      await Promise.resolve();
      await Promise.resolve();

      // Assert
      expect(mockReconnect).toHaveBeenCalled();
      expect(mockResumeCircuit).toHaveBeenCalled();
      expect(mockReload).toHaveBeenCalled();
    });

    test('should try resume circuit when reconnect fails', async () => {
      // Arrange
      mockReconnect.mockResolvedValue(false);
      mockResumeCircuit.mockResolvedValue(true);

      // Act
      retryButton.click();
      await Promise.resolve();
      await Promise.resolve();

      // Assert
      expect(mockReconnect).toHaveBeenCalled();
      expect(mockResumeCircuit).toHaveBeenCalled();
      expect(reconnectModal.close).toHaveBeenCalled();
    });

    test('should handle reconnect exception', async () => {
      // Arrange
      mockReconnect.mockRejectedValue(new Error('Network error'));
      const addEventListenerSpy = jest.spyOn(document, 'addEventListener');

      // Act
      retryButton.click();
      await Promise.resolve();

      // Assert
      expect(mockReconnect).toHaveBeenCalled();
      expect(addEventListenerSpy).toHaveBeenCalledWith(
        'visibilitychange',
        expect.any(Function)
      );

      addEventListenerSpy.mockRestore();
    });

    test('should remove visibilitychange listener before retrying', async () => {
      // Arrange
      mockReconnect.mockResolvedValue(true);
      const removeEventListenerSpy = jest.spyOn(document, 'removeEventListener');

      // Act
      retryButton.click();
      await Promise.resolve();

      // Assert
      expect(removeEventListenerSpy).toHaveBeenCalledWith(
        'visibilitychange',
        expect.any(Function)
      );

      removeEventListenerSpy.mockRestore();
    });
  });

  describe('Resume button behavior', () => {
    test('should call Blazor.resumeCircuit when resume button is clicked', async () => {
      // Arrange
      mockResumeCircuit.mockResolvedValue(true);

      // Act
      resumeButton.click();
      await Promise.resolve();

      // Assert
      expect(mockResumeCircuit).toHaveBeenCalled();
    });

    test('should reload page when resume fails', async () => {
      // Arrange
      mockResumeCircuit.mockResolvedValue(false);

      // Act
      resumeButton.click();
      await Promise.resolve();

      // Assert
      expect(mockResumeCircuit).toHaveBeenCalled();
      expect(mockReload).toHaveBeenCalled();
    });

    test('should reload page when resume throws exception', async () => {
      // Arrange
      mockResumeCircuit.mockRejectedValue(new Error('Resume error'));

      // Act
      resumeButton.click();
      await Promise.resolve();

      // Assert
      expect(mockResumeCircuit).toHaveBeenCalled();
      expect(mockReload).toHaveBeenCalled();
    });
  });

  describe('Blazor integration', () => {
    test('should have Blazor.reconnect available', () => {
      expect(globalThis.Blazor.reconnect).toBeDefined();
      expect(typeof globalThis.Blazor.reconnect).toBe('function');
    });

    test('should have Blazor.resumeCircuit available', () => {
      expect(globalThis.Blazor.resumeCircuit).toBeDefined();
      expect(typeof globalThis.Blazor.resumeCircuit).toBe('function');
    });

    test('should handle successful reconnect', async () => {
      // Arrange
      globalThis.Blazor.reconnect.mockResolvedValue(true);

      // Act
      const result = await globalThis.Blazor.reconnect();

      // Assert
      expect(result).toBe(true);
    });

    test('should handle failed reconnect', async () => {
      // Arrange
      globalThis.Blazor.reconnect.mockResolvedValue(false);

      // Act
      const result = await globalThis.Blazor.reconnect();

      // Assert
      expect(result).toBe(false);
    });

    test('should handle reconnect exception', async () => {
      // Arrange
      const error = new Error('Network error');
      globalThis.Blazor.reconnect.mockRejectedValue(error);

      // Act & Assert
      await expect(globalThis.Blazor.reconnect()).rejects.toThrow('Network error');
    });

    test('should handle successful resume', async () => {
      // Arrange
      globalThis.Blazor.resumeCircuit.mockResolvedValue(true);

      // Act
      const result = await globalThis.Blazor.resumeCircuit();

      // Assert
      expect(result).toBe(true);
    });

    test('should handle failed resume', async () => {
      // Arrange
      globalThis.Blazor.resumeCircuit.mockResolvedValue(false);

      // Act
      const result = await globalThis.Blazor.resumeCircuit();

      // Assert
      expect(result).toBe(false);
    });

    test('should handle resume exception', async () => {
      // Arrange
      const error = new Error('Resume failed');
      globalThis.Blazor.resumeCircuit.mockRejectedValue(error);

      // Act & Assert
      await expect(globalThis.Blazor.resumeCircuit()).rejects.toThrow('Resume failed');
    });
  });

  describe('Visibility change handling', () => {
    test('should retry when document becomes visible after failed state', async () => {
      // Arrange
      mockReconnect.mockResolvedValue(true);

      // Trigger failed state to add visibility listener
      const failedEvent = new CustomEvent('components-reconnect-state-changed', {
        detail: { state: 'failed' }
      });
      reconnectModal.dispatchEvent(failedEvent);

      // Act - simulate document becoming visible
      Object.defineProperty(document, 'visibilityState', {
        writable: true,
        configurable: true,
        value: 'visible'
      });

      const visibilityEvent = new Event('visibilitychange');
      document.dispatchEvent(visibilityEvent);
      await Promise.resolve();

      // Assert
      expect(mockReconnect).toHaveBeenCalled();
    });

    test('should not retry when document is hidden', async () => {
      // Arrange
      mockReconnect.mockResolvedValue(true);

      // Trigger failed state to add visibility listener
      const failedEvent = new CustomEvent('components-reconnect-state-changed', {
        detail: { state: 'failed' }
      });
      reconnectModal.dispatchEvent(failedEvent);

      mockReconnect.mockClear();

      // Act - simulate document becoming hidden
      Object.defineProperty(document, 'visibilityState', {
        writable: true,
        configurable: true,
        value: 'hidden'
      });

      const visibilityEvent = new Event('visibilitychange');
      document.dispatchEvent(visibilityEvent);
      await Promise.resolve();

      // Assert
      expect(mockReconnect).not.toHaveBeenCalled();
    });
  });
});
