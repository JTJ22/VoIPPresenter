import os
import sys
import librosa
import librosa.display
import matplotlib.pyplot as plt
import numpy as np

def main(audio_path, output_path):
    y, sr = librosa.load(audio_path)
    times = librosa.times_like(y, sr=sr)
    plt.figure(figsize=(10, 4))
    plt.plot(times, y)
    plt.title("Audio Waveform")
    plt.xlabel("Time (s)")
    plt.ylabel("Amplitude")
    plt.tight_layout()
    wav_path = os.path.join(output_path, "wave")
    plt.savefig(wav_path)
    plt.close()


    y, sr = librosa.load(audio_path)
    D = librosa.stft(y)
    S_db = librosa.amplitude_to_db(np.abs(D), ref=np.max)
    plt.figure(figsize=(10, 6))
    librosa.display.specshow(S_db, sr=sr, x_axis='time', y_axis='log')
    plt.colorbar(format='%+2.0f dB')
    plt.title('Spectrogram (Frequency over Time)')
    plt.xlabel('Time (s)')
    plt.ylabel('Frequency (Hz)')
    spec_path = os.path.join(output_path, "spec")
    plt.savefig(spec_path)
    plt.close()

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python generate_waveform.py <audio_file> <output_image>")
    else:
        main(sys.argv[1], sys.argv[2])


    # y, sr = librosa.load('audio.wav')
    # # Generate the spectrogram
    # D = librosa.amplitude_to_db(librosa.stft(y), ref=np.max)
    # # Display the spectrogram
    # plt.figure()
    # librosa.display.specshow(D, sr=sr, x_axis='time', y_axis='log')
    # plt.colorbar(format='%+2.0f dB')
    # plt.title('Spectrogram')
    # plt.tight_layout()
    # plt.show()