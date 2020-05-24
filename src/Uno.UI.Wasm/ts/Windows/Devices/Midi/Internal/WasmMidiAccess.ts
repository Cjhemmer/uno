﻿namespace Uno.Devices.Midi.Internal {
	export class WasmMidiAccess {
		private static midiAccess: WebMidi.MIDIAccess;

		private static dispatchRequest: (hasAccess: boolean) => number;

		public static request(): Promise<string> {			
			if (navigator.requestMIDIAccess) {
				console.log('This browser supports WebMIDI!');
				return navigator.requestMIDIAccess()
					.then(
						(midi: WebMidi.MIDIAccess) => {
							WasmMidiAccess.midiAccess = midi;
							return "true";
						},
						() => "false");
			}
			else {
				console.log('WebMIDI is not supported in this browser.');
				return Promise.resolve("false");
			}
		}

		public static getMidi(): WebMidi.MIDIAccess {			
			return this.midiAccess;
		}
	}
}
