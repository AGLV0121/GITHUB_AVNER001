
import sentence_transformers
from sentence_transformers import SentenceTransformer, util
import sys

model = SentenceTransformer('all-mpnet-base-v2')

def CompareSentences(sentence1, sentence2):
    """Compares two sentences using sentence-transformers."""

    # Get embedding vectors for both sentences
    sentence1_embedding = model.encode(sentence1, convert_to_tensor = True)
    sentence2_embedding = model.encode(sentence2, convert_to_tensor = True)

    # Calculate cosine similarity
    cosine_scores = util.cos_sim(sentence1_embedding, sentence2_embedding)
    similarity = cosine_scores.item()

    return similarity

if __name__ == "__main__":
    while True:
        try:
            line = input()
            if not line:  # Empty lines or EOF 
                break
            
# urllib.urlencode({'foo':raw_input('> ').decode('cp437').encode('utf8')})> áéíóúñ - or 'latin-1', 'cp1252'
# s = b.encode('utf-8').decode('utf-8') NO!
            sentence1, sentence2 = line.split("$$$")  # Split the input line into two sentences

            similarity = CompareSentences(sentence1, sentence2)
            
            print(similarity)  # Write the similarity score to StandardOutput
        except Exception as e:
            print(f"Error: {e}")  # Print error messages if any
