from cerevoice_eng import *
import wave
import winsound
import sys
import os

import time

text_to_speak = ""
file_count = ""
if (len(sys.argv) > 1):
    text_to_speak = sys.argv[1]
if (len(sys.argv) > 2):
    file_count = sys.argv[2]
if (text_to_speak is ""):
    text_to_speak = "No valid text input."

eng = CPRCEN_engine_load("license.lic", "cerevoice_heather_3.2.0_48k.voice")
# Create the following text as a .wav file
file_name = "out" + file_count + ".wav"
CPRCEN_engine_speak_to_file(eng, text_to_speak, file_name)
CPRCEN_engine_delete(eng)
# Play the .wav file
# winsound.PlaySound('out.wav', winsound.SND_PURGE)
winsound.PlaySound('out.wav', winsound.SND_FILENAME)
# Periodically check for a new output file
next_output_file_name = "out" + (file_count + 1) + ".wav"

# Delete the .wav file?
# os.remove('out.wav')

        
