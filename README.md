# Voyager-Golden-Record-Decoder
Source code to read and decode the images from the Voyager 1 space craft's Golden Record.  The source code is the demodulator portion of a very early modem.

With a little editing in photoshop to adjust tones, the recovered images look like this:

![alt text](https://i.imgur.com/Z5qRYMb.jpg)


Source Wav file here:

https://drive.google.com/drive/folders/0B0Swx_1rwA6XcFFLc29ncFJSZmM

This file captures a decoding attempt of the Voyager Golden Disk.  I have purposefully not read anything about the technology.  I am working strictly from the 384K sample wav data and an image of the disc itself with the inscriptions.

This is interesting because my background is mainly digital, not analogue, and these skills may never have developed or have died out in an alien race.  To me, this is as alien as it gets and I am not going to be looking up other examples as a means of assistance.  What I currently know is all I am allowed.

I haven't got a clue.

The scenario here is that this file of the recording has been sent via email along with some brief notes for analysis.  The attached note is as follows:


Primary Analysis:
-1 disc with etched information
-Appear to understand symbols/information
-Appear to encode symbols/information in various formats.
-Marking on the back of disc appear to relate to decoding - but not fully understood
-Some evidence of mathematics

Recordings of the disc's main contents have been made using many approaches including one or two derived from the markings of the disc.  The attached file is from the primary method we believe is encoded on the disc itself.  We have no idea how to decode this or how best to represent the information of the etching, other approaches are being tested by other teams. 

Data appears to be on two channels, on both channels the data can broken into 3 broad groups of structure, the latter of the 3 appears to have sub-structure. Data on each channel appears to be different, but similar.  Audio playback is a mixture of tones and electrical-like sounds.  The structure is indicative of information content.

We don't believe the data to be encrypted, nor was it addressed to anyone in particular.  It would seem the intention was just for someone to find it and decode given that instructions were on the disc.  This said, we have no broader notion of intent or the race that created it.   

Proposed theories so far:
Speech/Communication patterns
Writing
Encoded information

The latter proposal seems plausible as the etchings indicated how to read.  Deeper analysis of the file is required.





What follows from this point onwards is notes made in relation to analysis of the file itself.  Does it lead to a decode?

Note on original file with supplied recording:
Sampling rate: 384000 Sps
32 bit
1 sample = 2.6041666666666666666666666666667e-6 secs
Occupied FFT range: DC-100KHz

Brief analysis of first two major structures of the Left channel, labeled A and B respectively.

Section A:
1 Period: 3180 samples
1 Period = 0.00828125 secs
Hertz = 120.75471698113207547169811320755
Approx: 0.008 and 125Hz
Repeating structure, although some minor examples of change


Section B:
1 Period: 281 samples
1 Period = 7.3177083333333333333333333333333e-4 secs
Hertz = 1366.5480427046263345195729537366
Approx: 0.0007 and 1428.5714285714285714285714285714Hz, alternatively 0.0008 and 1250Hz - this latter value is a multiple of the earlier value, perhaps its a key to tolerances.
Repeating structure, although some minor examples of change


First thoughts:
The approximations may indicate that the recording equipment had wide tolerances.
Level of technology appears to be crude unless it is organic in nature.
Expect noise and an encoding to compensate.

Alternative view is that the recording equipment is highly accurate and may lend support to alternative decoding methods.

Presuming this method to be accurate, such signals are usually well structured and typically meaningless without a mapping to a representation. The only exception to this will be common mathematical notions based upon principles in physics which could be encoded.

Therefore, the working presumption must be that it is not data in a pure sense. At an electrical level, such signals can be used to drive technology and the representation arises from that output.  This needs to be the working hypothesis.

Given that we have already attempted audio and the results are meaningless, the approach will be to try it against technologies designed for various sensory inputs until sense is discovered.  One major presumption in all of this is that we share the same, or similar enough, form of sensory input.

Excluding sensory inputs not related to exteroception:
Proprioception
Interoception
Vestibular

While it is plausible that this recording encodes a BCI-style interface in an attempt to share a deeper sense of a spieces, or even as a primary means of communication, it seems unlikely and risky to attempt without broader analysis.

Sticking to exteroception, we are left with the following list:

Vision
Auditory - Checked. 
Tactile - Checked.  Etching proved of no value, could be encoded like braile though.
Olfactory
Taste

Vision seems like the path of least resistance given the ease with which it can be computed.  The final two require a machine with chemicals as an add-on. 

For now, the working hypotheses centres around two key themes, mathematics and vision with an electrical driving system.  

Mathematics seems unlikely given that we have already established intelligence.  It would be somewhat pointless providing us with math we already know, unless it was something really important and they we're confident it was not understood.  While that idea is appealing, it is somewhat contradicted by the initial assessment of engineering tolerances.  Given this, the first choice for exploration is vision with an electrical driving mechanism.

Our attention now turns to representation of vision.  Its a bit of a gamble that we'd share this sensory input, however, evolutionarly speaking there is a lot of common ground. There is also the question of how we perceive sensory input, we perceive it as continous stream and a single source, this may not be the case for other spieces.  As such, their notion of images/pictures could be fragmented both spatially and temporally.  What appears to us as a representation of a scene, could seem jumbled much in the same way as a dyslexic reading words.  As such, we have limited notion of what to expect or how to validate that.

The available bandwidth, selected medium and tolerances may indicate still images.  We have no idea how persistence of vision extends beyond our own species and a number of species on our planet.  As such, animation or video may not be realistic.

With all these presumptions in place, we can speculate that Sections A and B are some form of test/sync area, while Section C (which has the most variation) contains the data.  The notion of payloads of still images is reinforced by the packet like structure.


Extraction Notes:

Section C:
1 Period: 321 samples
1 Period = 8.3593750000000000000000000000001e-4 sec.
Hertz = 1196.2616822429906542056074766355

Period: 0.0005s
1/2000 or 2KHz


Section C: Sync
Period: 3096
Period: 0.0080625 secs
Hertz: 124.03100775193798449612403100775Hz



First - left channel
start: 6000208
samples to next beep: 2242363
samples to break in pulse rythm : 1928181

Peak-to-peak analysis (samples)
3300 (0.00859375 secs)
3098  (0.00806770833333333333333333333333 secs)
3300 (0.00859375 secs)
3097
3295
3093

About 724.04359057152082660639328382305 blocks between major tone changes
Abount 626.23611562195518025332900292303 blocks between major freq changes

Notes:
Wide drift in the peak-to-peak - Indicates either instability in the recording mechanism or variable rates.
Initial presumption will be instability, as this seems the easiest route.
A sliding window with peak detection will be required
The cover of the disc has value 512 which seems to fit into this range. Measurements indicate a second value of 384.
About 8.59 samples per pixel.

Recommended decoding strategy:
1. Create a bitmap 800 pixels in width, with a height of 384
2. Set file reader to start of segment
3. Read 3400 samples and define peaks
4. Divde the samples between the peaks into 384 groups, summing the magnitudes of each group
5. Map the magnitude to a color
6. Print the color to the screen advancing through all 384 in a column. 
7. Repeat steps 2- until segment is complete.
