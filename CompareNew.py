#CompareNew

import sentence_transformers as sbert
import numpy as np
import re
import time
import os

##################

MODEL_DIR = "f:/____MODELS/" 
pattern = r"[$]{3}"

##################

# Read the All-mpnet-V2 model from a local file
def read_model(file_path):
    model = sbert.SentenceTransformer(file_path)
    return model

##################

def mpnetSentenceComparerInit():
  modelSaveName = 'all-mpnet-base-v2.pkl'
  modelName = 'all-mpnet-base-v2'
# fileSave = os. getcwd() + '/'+ modelSaveName
  fileSave = MODEL_DIR + modelSaveName

# Save the All-mpnet-V2 model locally
  model = sbert.SentenceTransformer(modelName)

  file_exists = os.path.exists(modelSaveName)
  if file_exists:
    # Read the All-mpnet-V2 model from a local file
    model = read_model(fileSave)
  else:
    #save model locally
    save_model(model, fileSave)
    return model
  
# Save the All-mpnet-V2 model locally
def save_model(model, file_path):
    model.save(file_path)

model = mpnetSentenceComparerInit()

##################

# Define a function to compute the dot product
def computeDotProduct(embeddings1, embeddings2):
    dot_products = embeddings1 * embeddings2
    dot_products = dot_products.sum()
    return dot_products

##################

def getEmbeddings(sentence1, sentence2):
# generate embeddings
  embeddings1 = model.encode(sentence1)
  embeddings2 = model.encode(sentence2)
# return a tuple of embeddings
  return embeddings1, embeddings2

##################

#Computes the cosine similarity between two embeddings
# returns a float value representing the cosine similarity between the two embeddings
def cosine_similarity(embeddings1, embeddings2):
  dot_product = np.dot(embeddings1, embeddings2)
  magnitude1 = np.linalg.norm(embeddings1)
  magnitude2 = np.linalg.norm(embeddings2)
  return dot_product / (magnitude1 * magnitude2)

##################

def cosineSimilarity(embeddings1, embeddings2):
  cos_similarity = cosine_similarity(embeddings1, embeddings2)
  return cos_similarity

##################

def CompareSentences(sentence1, sentence2):
    """Compares two sentences using sentence-transformers."""
    embeddings_list = getEmbeddings(sentence1, sentence2)            
    cos_similarity = cosineSimilarity(embeddings_list[0],embeddings_list[1])
    return cos_similarity


if __name__ == "__main__":
    while True:
        try:
            line = input()
            if not line:  # Empty lines or EOF 
                break
            
# urllib.urlencode({'foo':raw_input('> ').decode('cp437').encode('utf8')})> áéíóúñ - or 'latin-1', 'cp1252'
# s = b.encode('utf-8').decode('utf-8') NO!

            # sentences =  line.split("$$$") # 2/10/2024
            sentences = re.split(pattern, line)
            if len(sentences) > 2:
              raise Exception("Length sentences exceeds 2")

            sentence1 = sentences[0]
            sentence2 = sentences[1]            
            similarity = CompareSentences(sentence1, sentence2)            
            print(similarity)  # Write the similarity score to StandardOutput
        except Exception as e:
            print(f"Error: {e}")  # Print error messages if any
